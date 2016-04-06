using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class RequestModuleTest
	{

		[Test]
		public void ShouldResolveNewAbsenceRequestHandler()
		{
			var builder = buildContainer();
			builder.Resolve<IHandleEvent<NewAbsenceRequestCreatedEvent>>().Should().Not.Be.Null();
		}

		private static ILifetimeScope buildContainer()
		{
			return buildContainer(CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader())));
		}

		private static ILifetimeScope buildContainer(IToggleManager toggleManager)
		{
			var builder = new ContainerBuilder();
			var configuration = new IocConfiguration(new IocArgs(new ConfigReader()), toggleManager);
			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterType<SetNoLicenseActivator>().As<ISetLicenseActivator>().SingleInstance(); //should probably use infratest attr for these tests instead.
			return builder.Build();
		}
	}
}