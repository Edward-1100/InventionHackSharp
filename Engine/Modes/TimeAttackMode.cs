using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;
using static EntityComponentSystemCSharp.EntityManager;

namespace MegaDungeon.Modes
{
	public class TimeAttackMode : IGameMode
	{
		public string Name => "Time Attack";
		int _turnBudget = 100;
		int _turnsElapsed = 0;
		Entity _exit;

		public void Initialize(IEngine engine)
		{
			var cell = engine.GetWalkableCell();
			_exit = engine.GetEntityManager().CreateEntity();
			_exit.AddComponent(new Location(){X = cell.X, Y = cell.Y});
			_exit.AddComponent(new Glyph(){glyph = engine.GetTileManager().GetGlyphNumByName("staircase down")});
			_exit.AddComponent(new Exit(){Locked = false});
		}

		public void OnTurn(IEngine engine)
		{
			_turnsElapsed++;
		}

		public GameModeOutcome CheckOutcome(IEngine engine)
		{
			var player = engine.GetPlayerLocation();
			var exitLoc = _exit.GetComponent<Location>();
			if(exitLoc.X == player.X && exitLoc.Y == player.Y)
			{
				return GameModeOutcome.Won;
			}
			if(_turnsElapsed >= _turnBudget)
			{
				return GameModeOutcome.Lost;
			}
			return GameModeOutcome.InProgress;
		}

		public string GetHudLine(IEngine engine)
		{
			var remaining = System.Math.Max(0, _turnBudget - _turnsElapsed);
			return $"Turns left: {remaining}";
		}
	}
}
