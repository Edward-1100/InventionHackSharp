using System.Linq;
using NUnit.Framework;
using MegaDungeon;
using MegaDungeon.Modes;

namespace Engine
{
	[TestFixture]
	public class GameModeRegistryTests
	{
		[Test]
		public void CreateDefaultHasFiveModes()
		{
			var registry = GameModeRegistry.CreateDefault();
			var names = registry.GetNames().ToList();

			Assert.AreEqual(5, names.Count);
			CollectionAssert.AreEquivalent(
				new[] {"Switch Hunt", "Extermination", "Endless", "Time Attack", "Scavenger Hunt"},
				names);
		}

		[Test]
		public void CreateReturnsModeByName()
		{
			var registry = GameModeRegistry.CreateDefault();
			
			var mode = registry.Create("Extermination");

			Assert.AreEqual("Extermination", mode.Name);
		}

		[Test]
		public void CreateReturnsNewInstanceEachTime()
		{
			var registry = GameModeRegistry.CreateDefault();

			var first = registry.Create("Switch Hunt");
			var second = registry.Create("Switch Hunt");

			Assert.AreNotSame(first, second);
		}

		[Test]
		public void RegisterAddsCustomMode()
		{
			var registry = new GameModeRegistry();
			registry.Register("Custom Mode", () => new EndlessMode());

			CollectionAssert.Contains(registry.GetNames().ToList(), "Custom Mode");
			Assert.IsNotNull(registry.Create("Custom Mode"));
		}
	}

	[TestFixture]
	public class ClassRegistryTests
	{
		[Test]
		public void CreateDefaultHasThreeClasses()
		{
			var registry = ClassRegistry.CreateDefault();
			var names = registry.GetNames().ToList();

			Assert.AreEqual(3, names.Count);
			CollectionAssert.AreEquivalent(new[] {"Warrior", "Scout", "Rogue"}, names);
		}

		[Test]
		public void CreateReturnsClassByName()
		{
			var registry = ClassRegistry.CreateDefault();

			var playerClass = registry.Create("Rogue");

			Assert.AreEqual("Rogue", playerClass.Name);
		}

		[Test]
		public void ClassesHaveDifferentStats()
		{
			var registry = ClassRegistry.CreateDefault();

			var warrior = registry.Create("Warrior").GetStats();
			var scout = registry.Create("Scout").GetStats();
			var rogue = registry.Create("Rogue").GetStats();

			Assert.IsFalse(
				warrior.MaxHealth == scout.MaxHealth &&
				warrior.Defense == scout.Defense &&
				warrior.Power == scout.Power &&
				warrior.Accuracy == scout.Accuracy &&
				warrior.Speed == scout.Speed &&
				warrior.SightRange == scout.SightRange);

			Assert.IsFalse(
				warrior.MaxHealth == rogue.MaxHealth &&
				warrior.Defense == rogue.Defense &&
				warrior.Power == rogue.Power &&
				warrior.Accuracy == rogue.Accuracy &&
				warrior.Speed == rogue.Speed &&
				warrior.SightRange == rogue.SightRange);
		}

		[Test]
		public void RegisterAddsCustomClass()
		{
			var registry = new ClassRegistry();
			registry.Register("Custom Class", () => new MegaDungeon.Classes.WarriorClass());

			CollectionAssert.Contains(registry.GetNames().ToList(), "Custom Class");
			Assert.IsNotNull(registry.Create("Custom Class"));
		}
	}

	[TestFixture]
	public class MapGeneratorRegistryTests
	{
		[Test]
		public void CreateDefaultHasThreeStrategies()
		{
			var registry = MapGeneratorRegistry.CreateDefault();
			var names = registry.GetNames().ToList();

			Assert.AreEqual(3, names.Count);
			CollectionAssert.AreEquivalent(new[] {"RandomRooms", "Cave", "BorderOnly"}, names);
		}

		[Test]
		public void CreateBuildsMapOfRequestedSize()
		{
			var registry = MapGeneratorRegistry.CreateDefault();

			var map = registry.Create("BorderOnly", 20, 15);

			Assert.AreEqual(20, map.Width);
			Assert.AreEqual(15, map.Height);
		}

		[Test]
		public void CreateWithUnknownNameThrows()
		{
			var registry = MapGeneratorRegistry.CreateDefault();

			var ex = Assert.Throws<System.ArgumentException>(() => registry.Create("NotRegistered", 10, 10));

			StringAssert.Contains("NotRegistered", ex.Message);
		}
	}
}
