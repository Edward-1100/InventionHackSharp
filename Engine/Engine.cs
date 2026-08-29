using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;
using EntityComponentSystemCSharp.Systems;
using RogueSharp;
using FeatureDetector;
using MegaDungeon.Contracts;
using static MegaDungeon.EngineConstants;
using static EntityComponentSystemCSharp.EntityManager;

namespace MegaDungeon
{
	public class Engine : IEngine, IGameSession
	{
		int _doorPercentChance = 50; // Percentage of possible doors that will actually spawn.
		Random random = new Random();
		int _width;
		int _height;
		TileType[,] _floor;
		TileType[,] _floor_view; // a copy to return to make _floor effectively readonly
		RogueSharp.IMap _map;
		ITileManager _tileManager;
		Location _playerLocation = new Location();
		List<int[]> _doorways = new List<int[]>();
		EntityManager _entityManager = new EntityManager();
		List<RogueSharp.ICell> _spawnLocations = new List<RogueSharp.ICell>();
		ISystemProvider _systemProvider = new SystemProvider();
		internal Queue<string> _messages = new Queue<string>();
		SightStat _playerSiteDistance;
		EntityManager.Entity _player;
		ActorManager _actorManager;
		HashSet<Point> _viewable;
		EngineLogger _logger;
		List<Point> _doors = new List<Point>();
		GameModeRegistry _gameModeRegistry = GameModeRegistry.CreateDefault();
		MapGeneratorRegistry _mapGeneratorRegistry = MapGeneratorRegistry.CreateDefault();
		Dictionary<string, string> _monsterPrototypes;
		IGameMode _gameMode;
		GameModeOutcome _gameModeOutcome = GameModeOutcome.InProgress;
		GameModeOutcome _previousGameModeOutcome = GameModeOutcome.InProgress;
		ClassRegistry _classRegistry = ClassRegistry.CreateDefault();
		IPlayerClass _playerClass;
		int _turnNumber = 0;
		public ISystemLogger GetLogger() {return _logger;}
		public RogueSharp.IMap GetMap() {return _map;}
		public ITileManager GetTileManager() {return _tileManager;}
		public EntityManager GetEntityManager() {return _entityManager;}
		public HashSet<Point> GetPlayerViewable() {return _viewable;}
		public Point GetPlayerLocation() {return new Point(){X = _playerLocation.X, Y = _playerLocation.Y};}
		int PLAYER;

		public HashSet<Point> Viewable
		{
			get => _viewable;
		}

		public EntityManager.Entity PlayerEntity
		{
			get => _player;
		}

		public Point PlayerLocation
		{
			get => new Point(_playerLocation.X, _playerLocation.Y);
		}

		public string[] Messages
		{
			get => _messages.ToArray();
		}

		public bool IsGameOver
		{
			get => (_player != null && !_player.HasComponent<Actor>()) || _gameModeOutcome != GameModeOutcome.InProgress;
		}

		public GameStateSnapshot GetSnapshot()
		{
			var actors = new List<ActorSnapshot>();
			foreach(var entity in _entityManager.GetAllEntitiesWithComponent<Location>())
			{
				var location = entity.GetComponent<Location>();
				var glyph = entity.GetComponent<Glyph>();
				if(glyph == null) {continue;}
				actors.Add(new ActorSnapshot()
				{
					EntityId = entity.Id,
					X = location.X,
					Y = location.Y,
					Glyph = glyph.glyph,
				});
			}

			return new GameStateSnapshot()
			{
				Tiles = _floor_view,
				RevealedTiles = (TileType[,])_floor,
				Viewable = _viewable,
				PlayerLocation = new Point(_playerLocation.X, _playerLocation.Y),
				Messages = _messages.ToArray(),
				Actors = actors,
				ModeHudLine = _gameMode.GetHudLine(this),
				ModeOutcome = _gameModeOutcome,
				TurnNumber = _turnNumber,
			};
		}

		public TileType[,] Floor
		{
			get{return _floor_view;}
		}

		public TileType[,] RevealedFloor
		{
			get{return (TileType[,])_floor;}
		}

		/// <summary>
		/// Dungeon floor engine, seperate from any UI.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="tileManager"></param>
		public Engine(int width, int height, ITileManager tileManager, string gameModeName = "Extermination", string playerClassName = "Warrior", string mapGeneratorName = "RandomRooms", string monstersPath = "Monsters", string monstersModsPath = "Mods/Monsters") {
			_width = width;
			_height = height;
			_floor = new TileType[width, height];
			_tileManager = tileManager;
			_logger = new EngineLogger(this);
			SetCommonTileGlyphs();

			_map = _mapGeneratorRegistry.Create(mapGeneratorName, width, height);
			InitCellGlyphs();
			_monsterPrototypes = MonsterLoader.LoadAll(monstersPath, monstersModsPath, _logger);
			_actorManager = new ActorManager(_entityManager);
			_playerClass = _classRegistry.Create(playerClassName);
			InitializePlayer();
			PlaceMonsters();
			_gameMode = _gameModeRegistry.Create(gameModeName);
			_gameMode.Initialize(this);
			UpdateViews();

			// Add systems that should run every turn here.
			_logger = new EngineLogger(this);

			// Setup systems to run. Order matters.
			_systemProvider.Register(new CombatSystem(this));
			_systemProvider.Register(new HealthSystem(this));
			_systemProvider.Register(new EnergySystem(this));
			_systemProvider.Register(new AISystem(this, BehaviorRegistry.CreateDefault(), DefaultAIRules()));
			_systemProvider.Register(new MovementSystem(this));
		}

		private void SetCommonTileGlyphs()
		{
			PLAYER = _tileManager.GetGlyphNumByName("valkyrie");
		}

		static List<AIRule> DefaultAIRules()
		{
			return new List<AIRule>()
			{
				new AIRule(){Condition="LowHealth", Behavior="Flee"},
				new AIRule(){Condition="NearPlayer", Behavior="Attack"},
				new AIRule(){Condition="Default", Behavior="Wander"},
			};
		}

		/// <summary>
		/// Given the player input, advance the game one turn.
		/// </summary>
		/// <param name="playerInput"></param>
		public void DoTurn(PlayerInput playerInput)
		{
			_turnNumber++;
			if(_player != null)
			{
				// Attach or modify any components needed to the player.
				var position = _player.GetComponent<Location>();
				if(INPUTMAP.ContainsKey(playerInput))
				{
					var delta = INPUTMAP[playerInput];
					var desired = delta + new Point(position.X, position.Y);
					var desiredComp = _player.GetComponent<Destination>();
					if(desiredComp == null)
					{
						desiredComp = new Destination();
						_player.AddComponent(desiredComp);
					}
					desiredComp.X = desired.X;
					desiredComp.Y = desired.Y;
				}
				_playerClass.BeforeTurn(this, _player);
			}

			// Cycle each entity through every system in turn.
			foreach(var entity in _entityManager.Entities)
			foreach(var system in _systemProvider.GetSystems())
			{
				system.Run(entity);
			}

			if(_player != null)
			{
				_playerClass.AfterTurn(this, _player);
			}

			UpdateViews();
			_gameMode.OnTurn(this);
			_gameModeOutcome = _gameMode.CheckOutcome(this);
			if(_gameModeOutcome != _previousGameModeOutcome)
			{
				if(_gameModeOutcome == GameModeOutcome.Won) {_logger.Log($"{_gameMode.Name}: you won!");}
				else if(_gameModeOutcome == GameModeOutcome.Lost) {_logger.Log($"{_gameMode.Name}: you lost.");}
				_previousGameModeOutcome = _gameModeOutcome;
			}
		}


		void InitializePlayer()
		{
			_player = _actorManager.GetPlayerActor(PLAYER, _playerClass.Name, _playerClass.GetStats());
			ICell cell = GetWalkableCell();
			_playerSiteDistance = _player.GetComponent<SightStat>();
			_playerLocation = new Location() { X = cell.X, Y = cell.Y };
			_player.AddComponent(_playerLocation);
		}

		public ICell GetWalkableCell()
		{
			var location = random.Next(0, _spawnLocations.Count);
			var cell = _spawnLocations[location];
			_spawnLocations.Remove(cell);
			return cell;
		}

		void PlaceMonsters() {
			if(_monsterPrototypes.Count == 0) {return;}
			var names = _monsterPrototypes.Keys.ToList();
			var numMonsters = RogueSharp.DiceNotation.Dice.Roll("1D6+3");
			for(int i = 0; i < numMonsters; i++) {
				var location = GetWalkableCell();
				var name = names[random.Next(names.Count)];
				var monster = _entityManager.NewEntityFromPrototype(_monsterPrototypes[name]);
				monster.AddComponent(new Location(){X = location.X, Y = location.Y});
				Debug.WriteLine($"Spawned {name} ({location.X},{location.Y})");
			}
		}

		/// <summary>
		/// Examine each cell and pick a suitable glyph for walls.
		/// </summary>
		void InitCellGlyphs()
		{
			int[,] mapArray = new int[_width, _height];
			for(int x = 0; x < _width; x++)
			{
				for(int y = 0; y < _height; y++)
				{
					var cell = _map.GetCell(x,y);
					if(cell.IsWalkable)
					{
						_spawnLocations.Add(cell);
					}
					mapArray[x,y] = cell.IsWalkable ? 0 : 1;
				}
			}

			var detector = new MapFeatureDetector(mapArray);
			var walls = detector.FindWalls();
			var doorways = detector.FindDoorways();

			for(int x = 0; x < _width; x++)
			{
				for(int y = 0; y < _height; y++)
				{
					_floor[x,y] = TileType.Dirt;
					if(mapArray[x,y] == 0) 
					{
						_floor[x,y] = TileType.Floor;
						if(doorways[x,y] > 0) {ConsiderAddDoor(x,y, (Orientation) doorways[x,y] -1);}
					}
					else 
					{
						if(walls[x,y] == 1) {_floor[x,y] = TileType.Wall;}
					}
				}
			}
		}

		void ConsiderAddDoor(int x, int y, Orientation orientation)
		{
			var doorRoll = random.Next(100);
			if(doorRoll < _doorPercentChance)
			{
				var doorPoint = new Point(x, y);
				foreach(Point d in _doors)
				{
					// No side by side doors
					if(PointDistance(doorPoint, d) == 1)
					{
						return;
					}
				}
				var entity = _entityManager.CreateEntity();
				var isDoor = new IsDoor(){Orientation = orientation, IsOpen = false};
				entity.AddComponent(isDoor);
				var location = new Location(){X = x, Y = y};
				entity.AddComponent(location);
				var glyph = new Glyph(){glyph = _tileManager.GetGlyphForTileType(TileType.Door)};
				entity.AddComponent(glyph);
				_map.SetCellProperties(x, y, isTransparent: false, isWalkable: true);
				_doors.Add(doorPoint);
				var cell = _map.GetCell(x, y);
				_spawnLocations.Remove(cell); // Remove doors as a spawn location
			}
		}

		double PointDistance(Point a, Point b)
		{
			var dx = a.X - b.X;
			var dy = a.Y - b.Y;
			var dist = Math.Sqrt((dx * dx) + (dy * dy));
			return dist;
		}

		/// <summary>
		/// Copy the private _floor to the public _floor_view
		/// </summary>
		void UpdateViews()
		{
			var circleView = GetFieldOfView(_playerLocation.X, _playerLocation.Y, _playerSiteDistance.Range, _map);
			var viewable = new List<Point>();
			foreach(var cell in circleView)
			{
				viewable.Add(new Point(cell.X, cell.Y));
				_map.AppendFov(cell.X, cell.Y, 0, true);
			}
			_viewable = new HashSet<Point>(viewable);

			if(_floor_view == null)
			{
				_floor_view = new TileType[_floor.GetLength(0), _floor.GetLength(1)];
			}

			for(int x = 0; x < _floor.GetLength(0); x++ )
			{
				for(int y = 0; y < _floor.GetLength(1); y++)
				{
					if(!_map.IsInFov(x, y))
					{
						_floor_view[x,y] = TileType.Dark;
					}
					else
					{
						_floor_view[x,y] = _floor[x,y];
					}
				}
			}
		}

		private static IEnumerable<ICell> GetFieldOfView( int x, int y, int selectionSize, IMap map )
		{
		List<ICell> circleFov = new List<ICell>();
		var fieldOfView = new FieldOfView( map );
		var cellsInFov = fieldOfView.ComputeFov( x, y, (int) ( selectionSize * 1.5 ), true );
		var circle = map.GetCellsInCircle( x, y, selectionSize ).ToList();
		foreach ( ICell cell in cellsInFov )
		{
			if ( circle.Contains( cell ) )
			{
			circleFov.Add( cell );
			}
		}
		return circleFov;
		}

	}

}
