using EntityComponentSystemCSharp;
using EntityComponentSystemCSharp.Components;

namespace MegaDungeon.Classes
{
	public class RogueClass : IPlayerClass
	{
		public string Name => "Rogue";
		int _bonusDamage = 4;
		bool _boosted;

		public ClassStats GetStats() => new ClassStats()
		{
			MaxHealth = 8,
			Defense = 6,
			Power = 14,
			Accuracy = 85,
			Speed = 11,
			SightRange = 5,
		};

		public void BeforeTurn(IEngine engine, EntityManager.Entity player)
		{
			_boosted = false;
			var destination = player.GetComponent<Destination>();
			if(destination == null) {return;}

			foreach(var entity in engine.GetEntityManager().GetAllEntitiesWithComponent<Location>())
			{
				var location = entity.GetComponent<Location>();
				if(location.X != destination.X || location.Y != destination.Y) {continue;}

				var life = entity.GetComponent<Life>();
				if(life != null && life.Health == life.MaxHealth)
				{
					player.GetComponent<AttackStat>().Power += _bonusDamage;
					_boosted = true;
				}
				return;
			}
		}

		public void AfterTurn(IEngine engine, EntityManager.Entity player)
		{
			if(!_boosted) {return;}
			player.GetComponent<AttackStat>().Power -= _bonusDamage;
			_boosted = false;
		}
	}
}
