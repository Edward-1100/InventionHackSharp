using EntityComponentSystemCSharp.Components;

namespace EntityComponentSystemCSharp.Systems
{
	public class GuardBehavior : IMonsterBehavior
	{
		public string Name => "Guard";

		public void Act(IEngine engine, EntityManager.Entity monster)
		{
			monster.RemoveComponent<Destination>();
		}
	}
}
