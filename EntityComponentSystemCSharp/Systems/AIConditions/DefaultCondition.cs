namespace EntityComponentSystemCSharp.Systems
{
	public class DefaultCondition : IAICondition
	{
		public string Name => "Default";
		public bool Matches(IEngine engine, EntityManager.Entity monster) => true;
	}
}
