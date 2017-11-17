using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			base.BeforeTest();
			Resolve<IMessageBrokerUnitOfWorkScope>().Start();
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			Resolve<IMessageBrokerUnitOfWorkScope>().End(null);
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}