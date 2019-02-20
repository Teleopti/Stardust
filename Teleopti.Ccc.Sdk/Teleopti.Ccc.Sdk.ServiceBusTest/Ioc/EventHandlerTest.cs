using System.Linq;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using SharpTestsEx;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Ioc
{
	[TestFixture]
	public class EventHandlerTest
	{
		[SetUp]
		public void Setup()
		{
			requestContainer = buildContainer();
		}

		[TearDown]
		public void Teardown()
		{
			if (requestContainer != null)
			{
				requestContainer.Dispose();
				requestContainer = null;
			}
		}

		private ILifetimeScope buildContainer()
		{
			var builder = new ContainerBuilder();
			var toggleManager = new FakeToggleManager();
			toggleManager.EnableAll();
			var iocConfig = new IocConfiguration(new IocArgs(new FakeConfigReader()), toggleManager);
			builder.RegisterModule(new NodeHandlersModule(iocConfig));
			builder.RegisterModule(new CommonModule(iocConfig));
			return builder.Build();
		}

		private ILifetimeScope requestContainer;

		[Test]
		public void PayrollExportHandlerNewTest()
		{
			requestContainer.Resolve<IHandleEvent<RunPayrollExportEvent>>();
		}

		[Test]
		public void ShouldResolveAllHandlers()
		{
			var allHandlers = requestContainer.ComponentRegistry.Registrations.SelectMany(r =>
				r.Activator.LimitType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IHandle<>)).Select(x => x));

			foreach (var handler in allHandlers)
			{
				requestContainer.Resolve(handler);
			}
		}
	}
}
