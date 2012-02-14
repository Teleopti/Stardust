using System;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest
{
	public static class ResolveExtensions
	{
		public static IResolve<T> Resolver<T>(this T instance)
		{
			return new GenericResolver<T>(() => instance);
		} 

		private class GenericResolver<T> : IResolve<T>
		{
			private readonly Func<T> _resolver;

			public GenericResolver(Func<T> resolver) {
				_resolver = resolver;
			}

			public T Invoke() { return _resolver.Invoke(); }
		}
	}
}