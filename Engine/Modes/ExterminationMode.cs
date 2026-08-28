using System.Linq;
using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;
using MegaDungeon.Contracts;

namespace MegaDungeon.Modes
{
	public class ExterminationMode : IGameMode
	{
		public string Name => "Extermination";

		public void Initialize(IEngine engine)
		{
		}

		public void OnTurn(IEngine engine)
		{
		}

		public GameModeOutcome CheckOutcome(IEngine engine)
		{
			var remaining = CountLivingMonsters(engine);
			return remaining == 0 ? GameModeOutcome.Won : GameModeOutcome.InProgress;
		}

		public string GetHudLine(IEngine engine)
		{
			return $"Monsters remaining: {CountLivingMonsters(engine)}";
		}

		int CountLivingMonsters(IEngine engine)
		{
			return engine.GetEntityManager().GetAllEntitiesWithComponent<Faction>()
				.Count(e => e.GetComponent<Faction>().Type == Factions.Monster && !e.HasComponent<Dead>());
		}
	}
}
