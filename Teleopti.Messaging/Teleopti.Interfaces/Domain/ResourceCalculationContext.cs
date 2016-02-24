using System;

namespace Teleopti.Interfaces.Domain
{
	public class ResourceCalculationContext : IDisposable
	{
		[ThreadStatic]
		private static IResourceCalculationDataContainerWithSingleOperation _container;
		
		public ResourceCalculationContext(IResourceCalculationDataContainerWithSingleOperation resources)
		{
			_container = resources;
		}

		public static IResourceCalculationDataContainerWithSingleOperation Fetch()

		private static T _container;
		
        /// <summary>
        /// Gets the container without creating a new if the instance doesn't exist
        /// </summary>
        /// <returns></returns>
        public static T Container()
        {
            return _container;
        }

		public static bool InContext
		{
			get { return _container != null; }
		}

		public void Dispose()
		{
			_container = null;
		}
	}
}