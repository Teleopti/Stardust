using System;
using System.Linq;
using LinFu.DynamicProxy;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public static class SetupThrowingTestDoublesForRepositories
	{
		private static readonly ProxyFactory proxyFactory = new ProxyFactory();

		public static void Execute(ISystem system)
		{
			foreach (var type in typeof(IPersonRepository).Assembly.GetExportedTypes().Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal) && t.IsInterface))
			{
				system.UseTestDouble(proxyFactory.CreateProxy(type, new throwRepositoryInterceptor())).For(type);
			}
		}

		private class throwRepositoryInterceptor : IInterceptor
		{
			public object Intercept(InvocationInfo info)
			{
				throw new RepositoriesMustNotBeUsedException();
			}
		}
	}
}