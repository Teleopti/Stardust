using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Ioc
{
	[TestFixture]
	public class EventHandlerTest : IocRegistrationTest
	{
		[Test]
		public void ShouldResolveAllHandlers([Values("Customer", "RC")] string toggleMode)
		{
			var container = InitContainer(toggleMode);
			var allHandlers = container.ComponentRegistry.Registrations.SelectMany(r =>
				r.Activator.LimitType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandle<>)).Select(x => x));

			foreach (var handler in allHandlers)
			{
				container.Resolve(handler);
			}
		}

		protected override IEnumerable<IModule> DefineModules(IocConfiguration iocConfiguration)
		{
			return new List<IModule>{new NodeHandlersModule(iocConfiguration), new CommonModule(iocConfiguration)};
		}
	}
}
