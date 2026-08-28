using EntityComponentSystemCSharp.Components;

namespace EntityComponentSystemCSharp.Systems
{
	public class LowHealthCondition : IAICondition
	{
		float _threshold = 0.3f;
		public string Name => "LowHealth";

		public bool Matches(IEngine engine, EntityManager.Entity monster)
		{
			var life = monster.GetComponent<Life>();
			if(life == null) {return false;}
			return life.Health <= life.MaxHealth * _threshold;
		}
	}
}
