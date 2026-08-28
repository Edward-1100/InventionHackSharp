using System;
using System.Collections.Generic;
using MegaDungeon.Classes;

namespace MegaDungeon
{
	public class ClassRegistry
	{
		Dictionary<string, Func<IPlayerClass>> _factories = new Dictionary<string, Func<IPlayerClass>>();

		public void Register(string name, Func<IPlayerClass> factory) => _factories[name] = factory;
		public IPlayerClass Create(string name) => _factories[name]();
		public IEnumerable<string> GetNames() => _factories.Keys;

		public static ClassRegistry CreateDefault()
		{
			var registry = new ClassRegistry();
			registry.Register("Warrior", () => new WarriorClass());
			registry.Register("Scout", () => new ScoutClass());
			registry.Register("Rogue", () => new RogueClass());
			return registry;
		}
	}
}
