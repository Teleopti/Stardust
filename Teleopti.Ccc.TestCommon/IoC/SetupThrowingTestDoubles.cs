using System;
using System.Linq;
using LinFu.DynamicProxy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public static class SetupThrowingTestDoubles
	{
		private static readonly ProxyFactory proxyFactory = new ProxyFactory();
		
		public static void ForAllRepositories(IIsolate isolate)
		{
			foreach (var type in typeof(IPersonRepository).Assembly.GetExportedTypes().Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal) && t.IsInterface))
			{
				For(isolate, type);
			}
		}
		
		public static void For(IIsolate isolate, Type componentType)
		{
			isolate.UseTestDouble(proxyFactory.CreateProxy(componentType, new ThrowInterceptor(componentType))).For(componentType);
		}
	}
}