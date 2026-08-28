using System.Collections.Generic;
using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;
using static EntityComponentSystemCSharp.EntityManager;

namespace MegaDungeon.Modes
{
	public class SwitchHuntMode : IGameMode
	{
		public string Name => "Switch Hunt";
		int _switchCount = 4;
		List<Entity> _switches = new List<Entity>();
		Entity _exit;

		public void Initialize(IEngine engine)
		{
			for(int i = 0; i < _switchCount; i++)
			{
				var cell = engine.GetWalkableCell();
				var entity = engine.GetEntityManager().CreateEntity();
				entity.AddComponent(new Location(){X = cell.X, Y = cell.Y});
				entity.AddComponent(new Glyph(){glyph = engine.GetTileManager().GetGlyphNumByName("crystal ball")});
				entity.AddComponent<Switch>();
				_switches.Add(entity);
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
			for(int i = _switches.Count - 1; i >= 0; i--)
			{
				var loc = _switches[i].GetComponent<Location>();
				if(loc.X == player.X && loc.Y == player.Y)
				{
					engine.GetEntityManager().DestroyEntity(_switches[i]);
					_switches.RemoveAt(i);
				}
			}

			if(_switches.Count == 0)
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
			var flipped = _switchCount - _switches.Count;
			return $"Switches: {flipped} / {_switchCount}";
		}
	}
}
