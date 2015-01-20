using System.Configuration;
using Autofac;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class InfrastructureIoCTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterInstance(new MutableFakeCurrentHttpContext()).AsSelf().As<ICurrentHttpContext>().SingleInstance();
			builder.RegisterInstance(new MutableFakeCurrentTeleoptiPrincipal()).AsSelf().As<ICurrentTeleoptiPrincipal>().SingleInstance();
			builder.RegisterInstance(fakeConfig()).As<IConfigReader>().AsSelf().SingleInstance();
		}

		private static FakeConfigReader fakeConfig()
		{
			return new FakeConfigReader
			{
				ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests)
				}
			};
		}
	}
}