using System.Collections.Generic;

namespace EntityComponentSystemCSharp.Systems
{
	public class SystemProvider : ISystemProvider
	{
		List<ISystem> _systems = new List<ISystem>();

		public void Register(ISystem system) => _systems.Add(system);
		public IEnumerable<ISystem> GetSystems() => _systems;
	}
}
