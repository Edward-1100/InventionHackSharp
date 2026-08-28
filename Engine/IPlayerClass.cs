using EntityComponentSystemCSharp;

namespace MegaDungeon
{
	public class ClassStats
	{
		public int MaxHealth;
		public int Defense;
		public int Power;
		public int Accuracy;
		public float Speed;
		public int SightRange;
	}

	public interface IPlayerClass
	{
		string Name {get;}
		ClassStats GetStats();
		void BeforeTurn(IEngine engine, EntityManager.Entity player);
		void AfterTurn(IEngine engine, EntityManager.Entity player);
	}
}
