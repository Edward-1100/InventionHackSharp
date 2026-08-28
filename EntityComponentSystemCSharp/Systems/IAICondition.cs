namespace EntityComponentSystemCSharp.Systems
{
	public interface IAICondition
	{
		string Name {get;}
		bool Matches(IEngine engine, EntityManager.Entity monster);
	}
}
