using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;

namespace MegaDungeon.Classes
{
	public class ScoutClass : IPlayerClass
	{
		public string Name => "Scout";
		float _bonusEnergyPerTurn = 3f;

		public ClassStats GetStats() => new ClassStats()
		{
			MaxHealth = 7,
			Defense = 8,
			Power = 10,
			Accuracy = 75,
			Speed = 14,
			SightRange = 8,
		};

		public void BeforeTurn(IEngine engine, EntityManager.Entity player)
		{
		}

		public void AfterTurn(IEngine engine, EntityManager.Entity player)
		{
			var actor = player.GetComponent<Actor>();
			if(actor == null) {return;}
			actor.Energy += _bonusEnergyPerTurn;
		}
	}
}
