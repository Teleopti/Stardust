using System;
using System.Linq;
using LinFu.DynamicProxy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public static class SetupThrowingTestDoublesForRepositories
	{
		private static readonly ProxyFactory proxyFactory = new ProxyFactory();
		
		public static void Execute(IIsolate isolate)
		{
			foreach (var type in typeof(IPersonRepository).Assembly.GetExportedTypes().Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal) && t.IsInterface))
			{
				Execute(isolate, type);
			}
		}
		
		public static void Execute(IIsolate isolate, Type repositoryInterfaceType)
		{
			isolate.UseTestDouble(proxyFactory.CreateProxy(repositoryInterfaceType, new ThrowRepositoryInterceptor(repositoryInterfaceType))).For(repositoryInterfaceType);
		}
	}
}