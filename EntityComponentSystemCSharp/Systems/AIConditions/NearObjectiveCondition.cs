using EntityComponentSystemCSharp.Components;

namespace EntityComponentSystemCSharp.Systems
{
	public class NearObjectiveCondition : IAICondition
	{
		int _radius = 4;
		public string Name => "NearObjective";

		public bool Matches(IEngine engine, EntityManager.Entity monster)
		{
			var location = monster.GetComponent<Location>();
			if(location == null) {return false;}

			foreach(var entity in engine.GetEntityManager().GetAllEntitiesWithComponent<Location>())
			{
				if(!entity.HasComponent<Exit>() && !entity.HasComponent<Switch>() && !entity.HasComponent<GoldPickup>()) {continue;}
				var objectiveLocation = entity.GetComponent<Location>();
				var dx = objectiveLocation.X - location.X;
				var dy = objectiveLocation.Y - location.Y;
				if((dx * dx) + (dy * dy) <= _radius * _radius) {return true;}
			}
			return false;
		}
	}
}
