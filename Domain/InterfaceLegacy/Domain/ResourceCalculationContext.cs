using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public class ResourceCalculationContext : IDisposable
	{
		[ThreadStatic]
		private static Lazy<IResourceCalculationDataContainerWithSingleOperation> _container;
		
		public ResourceCalculationContext(Lazy<IResourceCalculationDataContainerWithSingleOperation> resources)
		{
			_container = resources;
		}
		public static IResourceCalculationDataContainerWithSingleOperation Fetch()
		{
			return _container.Value;
		}

		public static bool InContext => _container != null;

		public static bool PrimarySkillMode()
		{
			return InContext && Fetch().PrimarySkillMode;
		}

		public void Dispose()
		{
			_container = null;
		}
	}
}