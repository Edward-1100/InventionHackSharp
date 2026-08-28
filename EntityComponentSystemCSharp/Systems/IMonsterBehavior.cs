namespace EntityComponentSystemCSharp.Systems
{
	public interface IMonsterBehavior
	{
		string Name {get;}
		void Act(IEngine engine, EntityManager.Entity monster);
	}
}
