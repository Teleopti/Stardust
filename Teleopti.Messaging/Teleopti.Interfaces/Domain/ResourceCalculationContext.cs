using System;

namespace Teleopti.Interfaces.Domain
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

		public static bool InContext
		{
			get { return _container != null; }
		}

		public static bool PrimarySkillMode()
		{
			return InContext && Fetch().PrimarySkillMode;
		}

		public static void Clear()
		{
			new ResourceCalculationContext(null);
		}

		public void Dispose()
		{
			_container = null;
		}
	}
}