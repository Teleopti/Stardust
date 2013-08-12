using System;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
	public class ResourceCalculationContext<T> : IDisposable where T : class
	{
		[ThreadStatic]
		private static T _container;

		public static T Container(Func<T> createContainer)
		{
			return _container ?? createContainer();
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