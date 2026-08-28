using System.Collections.Generic;
using System.Linq;
using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;
using static EntityComponentSystemCSharp.EntityManager;

namespace MegaDungeon.Modes
{
	public class ScavengerHuntMode : IGameMode
	{
		public string Name => "Scavenger Hunt";
		int _goldQuota = 30;
		int _goldPileCount = 6;
		List<Entity> _goldPiles = new List<Entity>();
		Entity _exit;

		public void Initialize(IEngine engine)
		{
			for(int i = 0; i < _goldPileCount; i++)
			{
				var cell = engine.GetWalkableCell();
				var pile = engine.GetEntityManager().CreateEntity();
				pile.AddComponent(new Location(){X = cell.X, Y = cell.Y});
				pile.AddComponent(new Glyph(){glyph = engine.GetTileManager().GetGlyphNumByName("gold piece")});
				pile.AddComponent(new GoldPickup(){Value = _goldQuota / _goldPileCount});
				_goldPiles.Add(pile);
			}

			var exitCell = engine.GetWalkableCell();
			_exit = engine.GetEntityManager().CreateEntity();
			_exit.AddComponent(new Location(){X = exitCell.X, Y = exitCell.Y});
			_exit.AddComponent(new Glyph(){glyph = engine.GetTileManager().GetGlyphNumByName("staircase down")});
			_exit.AddComponent(new Exit(){Locked = true});
		}

		public void OnTurn(IEngine engine)
		{
			var player = engine.GetPlayerLocation();
			var playerActor = GetPlayerActor(engine);
			if(playerActor == null) {return;}

			for(int i = _goldPiles.Count - 1; i >= 0; i--)
			{
				var loc = _goldPiles[i].GetComponent<Location>();
				if(loc.X == player.X && loc.Y == player.Y)
				{
					var pickup = _goldPiles[i].GetComponent<GoldPickup>();
					playerActor.Gold += pickup.Value;
					engine.GetEntityManager().DestroyEntity(_goldPiles[i]);
					_goldPiles.RemoveAt(i);
				}
			}

			if(playerActor.Gold >= _goldQuota)
			{
				_exit.GetComponent<Exit>().Locked = false;
			}
		}

		public GameModeOutcome CheckOutcome(IEngine engine)
		{
			var player = engine.GetPlayerLocation();
			var exitLoc = _exit.GetComponent<Location>();
			if(!_exit.GetComponent<Exit>().Locked && exitLoc.X == player.X && exitLoc.Y == player.Y)
			{
				return GameModeOutcome.Won;
			}
			return GameModeOutcome.InProgress;
		}

		public string GetHudLine(IEngine engine)
		{
			var gold = GetPlayerActor(engine)?.Gold ?? 0;
			return $"Gold: {gold} / {_goldQuota}";
		}

		Actor GetPlayerActor(IEngine engine)
		{
			return engine.GetEntityManager().GetAllEntitiesWithComponent<Player>().FirstOrDefault()?.GetComponent<Actor>();
		}
	}
}
