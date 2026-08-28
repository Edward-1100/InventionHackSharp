using EntityComponentSystemCSharp.Components;

namespace EntityComponentSystemCSharp.Systems
{
	public class AttackBehavior : IMonsterBehavior
	{
		public string Name => "Attack";

		public void Act(IEngine engine, EntityManager.Entity monster)
		{
			var playerLocation = engine.GetPlayerLocation();
			var desired = monster.GetComponent<Destination>();
			if(desired == null) {desired = monster.AddComponent<Destination>();}
			desired.X = playerLocation.X;
			desired.Y = playerLocation.Y;
		}
	}
}
