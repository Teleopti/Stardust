using System;

namespace Teleopti.Interfaces.Domain
{
	public class ResourceCalculationContext<T> : IDisposable where T : class
	{
		[ThreadStatic]
		private static T _container;

        /// <summary>
        /// Gets the container without creating a new if the instance doesn't exist
        /// </summary>
        /// <returns></returns>
        public static T Container()
        {
            return _container;
        }

		public ResourceCalculationContext(T resources)
		{
			_container = resources;
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