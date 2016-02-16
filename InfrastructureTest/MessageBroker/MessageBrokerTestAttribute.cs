using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerTestAttribute : InfrastructureTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble(new FakeSignalR()).For<ISignalR>();
		}

		protected override void AfterTest()
		{
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}