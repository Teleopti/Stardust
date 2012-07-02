using System;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Core.Mapping
{
	public class FakeMappingDependency<T> : IResolve<T>
	{
		private readonly Func<T> _dependency;
		public FakeMappingDependency(T instance) { _dependency = () => instance; }
		public FakeMappingDependency(Func<T> lazy) { _dependency = lazy; }
		public T Invoke() { return _dependency.Invoke(); }
	}

	public static class Depend
	{
		public static IResolve<T> On<T>(T mappingDependency) { return new FakeMappingDependency<T>(mappingDependency); }
		public static IResolve<T> On<T>(Func<T> lazyMappingDependency) { return new FakeMappingDependency<T>(lazyMappingDependency); }
	}
}
