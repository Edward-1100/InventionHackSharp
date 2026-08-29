using System.Collections.Generic;
using NUnit.Framework;
using EntityComponentSystemCSharp.Components;
using EntityComponentSystemCSharp.Systems;
using MegaDungeon.Contracts;

namespace EntityComponentSystemCSharp
{
	[TestFixture]
	public class AISystemOrderingTests
	{
		class FakeCondition : IAICondition
		{
			public string Name {get;}
			bool _matches;
			public FakeCondition(string name, bool matches) { Name = name; _matches = matches; }
			public bool Matches(IEngine engine, EntityManager.Entity monster) => _matches;
		}

		class RecordingBehavior : IMonsterBehavior
		{
			public string Name {get;}
			public int ActCount;
			public RecordingBehavior(string name) { Name = name; }
			public void Act(IEngine engine, EntityManager.Entity monster) => ActCount++;
		}

		MockEngine _engine;
		BehaviorRegistry _registry;
		RecordingBehavior _wander;
		RecordingBehavior _attack;
		RecordingBehavior _flee;

		[SetUp]
		public void SetUp()
		{
			_engine = new MockEngine(new EntityManager(), new MockLogger(), new MockMap());

			_registry = new BehaviorRegistry();
			_wander = new RecordingBehavior("Wander");
			_attack = new RecordingBehavior("Attack");
			_flee = new RecordingBehavior("Flee");
			_registry.RegisterBehavior(_wander);
			_registry.RegisterBehavior(_attack);
			_registry.RegisterBehavior(_flee);
		}

		EntityManager.Entity CreateMonster(EntityManager em, List<AIRule> rules = null)
		{
			var entity = em.CreateEntity();
			entity.AddComponent(new WanderingMonster());
			entity.AddComponent(new Location());
			if(rules != null)
			{
				var profile = entity.AddComponent<AIProfile>();
				profile.Rules = rules;
			}
			return entity;
		}

		[Test]
		public void DefaultRuleRunsWander()
		{
			_registry.RegisterCondition(new FakeCondition("Default", true));
			
			var defaultRules = new List<AIRule> { new AIRule { Condition = "Default", Behavior = "Wander" } };
			var system = new AISystem(_engine, _registry, defaultRules);

			var em = new EntityManager();
			var monster = CreateMonster(em);

			system.Run(monster);

			Assert.AreEqual(1, _wander.ActCount);
			Assert.AreEqual(0, _attack.ActCount);
		}

		[Test]
		public void FirstMatchingRuleWins()
		{
			_registry.RegisterCondition(new FakeCondition("NearPlayer", true));
			_registry.RegisterCondition(new FakeCondition("Default", true));

			var rules = new List<AIRule>
			{
				new AIRule { Condition = "NearPlayer", Behavior = "Attack" },
				new AIRule { Condition = "Default", Behavior = "Wander" }
			};
			var system = new AISystem(_engine, _registry, new List<AIRule>());

			var em = new EntityManager();
			var monster = CreateMonster(em, rules);

			system.Run(monster);

			Assert.AreEqual(1, _attack.ActCount);
			Assert.AreEqual(0, _wander.ActCount);
		}

		[Test]
		public void NonMatchingRuleFallsThrough()
		{
			_registry.RegisterCondition(new FakeCondition("LowHealth", false));
			_registry.RegisterCondition(new FakeCondition("Default", true));

			var rules = new List<AIRule>
			{
				new AIRule { Condition = "LowHealth", Behavior = "Flee" },
				new AIRule { Condition = "Default", Behavior = "Wander" }
			};
			var system = new AISystem(_engine, _registry, new List<AIRule>());

			var em = new EntityManager();
			var monster = CreateMonster(em, rules);

			system.Run(monster);

			Assert.AreEqual(0, _flee.ActCount);
			Assert.AreEqual(1, _wander.ActCount);
		}

		[Test]
		public void PerMonsterRulesOverrideGlobalDefaults()
		{
			_registry.RegisterCondition(new FakeCondition("Default", true));

			var globalDefaultRules = new List<AIRule> { new AIRule { Condition = "Default", Behavior = "Wander" } };
			var perMonsterRules = new List<AIRule> { new AIRule { Condition = "Default", Behavior = "Attack" } };
			var system = new AISystem(_engine, _registry, globalDefaultRules);

			var em = new EntityManager();
			var monster = CreateMonster(em, perMonsterRules);

			system.Run(monster);

			Assert.AreEqual(1, _attack.ActCount);
			Assert.AreEqual(0, _wander.ActCount);
		}

		[Test]
		public void NoProfileFallsBackToGlobalDefaults()
		{
			_registry.RegisterCondition(new FakeCondition("Default", true));

			var globalDefaultRules = new List<AIRule> { new AIRule { Condition = "Default", Behavior = "Wander" } };
			var system = new AISystem(_engine, _registry, globalDefaultRules);

			var em = new EntityManager();
			var monster = CreateMonster(em);

			system.Run(monster);

			Assert.AreEqual(1, _wander.ActCount);
		}

		[Test]
		public void EmptyProfileFallsBackToGlobalDefaults()
		{
			_registry.RegisterCondition(new FakeCondition("Default", true));

			var globalDefaultRules = new List<AIRule> { new AIRule { Condition = "Default", Behavior = "Wander" } };
			var system = new AISystem(_engine, _registry, globalDefaultRules);

			var em = new EntityManager();
			var monster = CreateMonster(em, new List<AIRule>());

			system.Run(monster);

			Assert.AreEqual(1, _wander.ActCount);
		}

		[Test]
		public void NonMonsterEntitiesAreSkipped()
		{
			_registry.RegisterCondition(new FakeCondition("Default", true));
			var globalDefaultRules = new List<AIRule> { new AIRule { Condition = "Default", Behavior = "Wander" } };
			var system = new AISystem(_engine, _registry, globalDefaultRules);

			var em = new EntityManager();
			var entity = em.CreateEntity();

			system.Run(entity);

			Assert.AreEqual(0, _wander.ActCount);
		}
	}
}
