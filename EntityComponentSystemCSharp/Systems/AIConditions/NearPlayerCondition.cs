using EntityComponentSystemCSharp.Components;
using RogueSharp;

namespace EntityComponentSystemCSharp.Systems
{
	public class NearPlayerCondition : IAICondition
	{
		public string Name => "NearPlayer";

		public bool Matches(IEngine engine, EntityManager.Entity monster)
		{
			var location = monster.GetComponent<Location>();
			if(location == null) {return false;}
			return engine.GetPlayerViewable().Contains(new Point(location.X, location.Y));
		}
	}
}
