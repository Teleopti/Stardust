using Autofac;
using NUnit.Framework;
using Rhino.ServiceBus;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	public class InitializeApplicationResolveTest
	{
		[Test]
		public void ShouldResolveInitializeApplication()
		{ 
			using (var container = new ContainerBuilder().Build())
			{
				new ContainerConfiguration(container, new FalseToggleManager()).Configure(null);
				container.Resolve<IInitializeApplication>().Should().Not.Be.Null();
			}
		}
	}
}