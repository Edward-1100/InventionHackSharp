using System;
using System.Collections.Generic;
using MegaDungeon.Modes;

namespace MegaDungeon
{
	public class GameModeRegistry
	{
		Dictionary<string, Func<IGameMode>> _factories = new Dictionary<string, Func<IGameMode>>();

		public void Register(string name, Func<IGameMode> factory) => _factories[name] = factory;
		public IGameMode Create(string name) => _factories[name]();
		public IEnumerable<string> GetNames() => _factories.Keys;

		public static GameModeRegistry CreateDefault()
		{
			var registry = new GameModeRegistry();
			registry.Register("Switch Hunt", () => new SwitchHuntMode());
			registry.Register("Extermination", () => new ExterminationMode());
			registry.Register("Endless", () => new EndlessMode());
			registry.Register("Time Attack", () => new TimeAttackMode());
			registry.Register("Scavenger Hunt", () => new ScavengerHuntMode());
			return registry;
		}
	}
}
