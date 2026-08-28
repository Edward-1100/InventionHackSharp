using System.Collections.Generic;

namespace EntityComponentSystemCSharp.Systems
{
	public interface ISystemProvider
	{
		void Register(ISystem system);
		IEnumerable<ISystem> GetSystems();
	}
}
