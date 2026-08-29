using System.Linq;
using NUnit.Framework;
using EntityComponentSystemCSharp.Systems;

namespace EntityComponentSystemCSharp
{
	[TestFixture]
	public class SystemProviderTests
	{
		class RecordingSystem : ISystem
		{
			public int RunCount;
			public void Run(EntityManager.Entity entity) => RunCount++;
		}

		[Test]
		public void SystemsReturnedInRegistrationOrder()
		{
			ISystemProvider provider = new SystemProvider();
			var first = new RecordingSystem();
			var second = new RecordingSystem();
			var third = new RecordingSystem();

			provider.Register(first);
			provider.Register(second);
			provider.Register(third);

			var systems = provider.GetSystems().ToList();

			Assert.AreEqual(3, systems.Count);
			Assert.AreSame(first, systems[0]);
			Assert.AreSame(second, systems[1]);
			Assert.AreSame(third, systems[2]);
		}

		[Test]
		public void NoSystemsReturnsEmpty()
		{
			ISystemProvider provider = new SystemProvider();
			Assert.IsEmpty(provider.GetSystems());
		}

		[Test]
		public void RegisteredSystemRuns()
		{
			ISystemProvider provider = new SystemProvider();
			var system = new RecordingSystem();
			provider.Register(system);

			var em = new EntityManager();
			var entity = em.CreateEntity();

			foreach(var s in provider.GetSystems())
			{
				s.Run(entity);
			}

			Assert.AreEqual(1, system.RunCount);
		}
	}

	[TestFixture]
	public class BehaviorRegistryTests
	{
		class FakeCondition : IAICondition
		{
			public string Name {get;}
			public bool Result;
			public FakeCondition(string name, bool result) { Name = name; Result = result; }
			public bool Matches(IEngine engine, EntityManager.Entity monster) => Result;
		}

		class FakeBehavior : IMonsterBehavior
		{
			public string Name {get;}
			public bool WasActed;
			public FakeBehavior(string name) { Name = name; }
			public void Act(IEngine engine, EntityManager.Entity monster) => WasActed = true;
		}

		[Test]
		public void GetConditionReturnsRegistered()
		{
			var registry = new BehaviorRegistry();
			var condition = new FakeCondition("Custom", true);
			
			registry.RegisterCondition(condition);

			Assert.AreSame(condition, registry.GetCondition("Custom"));
		}

		[Test]
		public void GetBehaviorReturnsRegistered()
		{
			var registry = new BehaviorRegistry();
			var behavior = new FakeBehavior("Custom");

			registry.RegisterBehavior(behavior);

			Assert.AreSame(behavior, registry.GetBehavior("Custom"));
		}

		[Test]
		public void CreateDefaultRegistersBuiltIns()
		{
			var registry = BehaviorRegistry.CreateDefault();

			Assert.IsNotNull(registry.GetCondition("Default"));
			Assert.IsNotNull(registry.GetCondition("NearPlayer"));
			Assert.IsNotNull(registry.GetCondition("NearObjective"));
			Assert.IsNotNull(registry.GetCondition("LowHealth"));

			Assert.IsNotNull(registry.GetBehavior("Wander"));
			Assert.IsNotNull(registry.GetBehavior("Attack"));
			Assert.IsNotNull(registry.GetBehavior("Guard"));
			Assert.IsNotNull(registry.GetBehavior("Flee"));
		}

		[Test]
		public void RegisterReplacesSameName()
		{
			var registry = new BehaviorRegistry();
			registry.RegisterCondition(new FakeCondition("Custom", false));
			var replacement = new FakeCondition("Custom", true);
			registry.RegisterCondition(replacement);

			Assert.AreSame(replacement, registry.GetCondition("Custom"));
		}
	}
}
