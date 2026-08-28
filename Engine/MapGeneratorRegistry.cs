using System;
using System.Collections.Generic;

namespace MegaDungeon {
	public class MapGeneratorRegistry {
		Dictionary<string, Func<int, int, RogueSharp.IMap>> _factories = new Dictionary<string, Func<int, int, RogueSharp.IMap>>();

		public void Register(string name, Func<int, int, RogueSharp.IMap> factory) => _factories[name] = factory;
		public IEnumerable<string> GetNames() => _factories.Keys;

		public RogueSharp.IMap Create(string name, int width, int height) {
			if(!_factories.ContainsKey(name)) {
				throw new ArgumentException($"No map generator is registered under the name '{name}'.", nameof(name));
			}
			return _factories[name](width, height);
		}

		public static MapGeneratorRegistry CreateDefault() {
			var registry = new MapGeneratorRegistry();
			registry.Register("RandomRooms", (width, height) => {
				var maxRooms = (int) Math.Sqrt(height * width) * 2;
				var strategy = new RogueSharp.MapCreation.RandomRoomsMapCreationStrategy<RogueSharp.Map>(width, height, maxRooms, 10, 5);
				return strategy.CreateMap();
			});
			registry.Register("Cave", (width, height) => {
				var strategy = new CaveMapCreationStrategy<RogueSharp.Map>(width, height);
				return strategy.CreateMap();
			});
			registry.Register("BorderOnly", (width, height) => {
				var strategy = new RogueSharp.MapCreation.BorderOnlyMapCreationStrategy<RogueSharp.Map>(width, height);
				return strategy.CreateMap();
			});
			return registry;
		}
	}
}
