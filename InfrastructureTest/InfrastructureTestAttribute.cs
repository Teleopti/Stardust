using System.Configuration;
using Autofac;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class InfrastructureTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterInstance(new FakeDatabaseConnectionStringHandler()).As<IDatabaseConnectionStringHandler>();

			builder.RegisterInstance(new FakeConfigReader
			{
				ConnectionStrings = new ConnectionStringSettingsCollection
				{
					new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests)
				}
			}).As<IConfigReader>().AsSelf().SingleInstance();

		}
	}
}