using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public class ResourceCalculationContext : IDisposable
	{
		[ThreadStatic]
		private static Lazy<IResourceCalculationDataContainerWithSingleOperation> _container;
		private static IResourceCalculationDataContainerWithSingleOperation _containerEager;
		
		public ResourceCalculationContext(Lazy<IResourceCalculationDataContainerWithSingleOperation> resources)
		{
			_container = resources;
		}

		public ResourceCalculationContext(IResourceCalculationDataContainerWithSingleOperation resources)
		{
			_containerEager = resources;
		}

		public static IResourceCalculationDataContainerWithSingleOperation Fetch()
		{
			return _containerEager ?? _container.Value;
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