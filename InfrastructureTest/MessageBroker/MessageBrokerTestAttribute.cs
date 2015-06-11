using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerTestAttribute : InfrastructureTestAttribute
	{
		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.UseTestDouble(new FakeSignalR()).For<ISignalR>();
		}

		protected override void AfterTest()
		{
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}