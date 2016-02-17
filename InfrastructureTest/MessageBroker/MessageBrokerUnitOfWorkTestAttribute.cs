using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.InfrastructureTest.MessageBroker
{
	public class MessageBrokerUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			base.BeforeTest();
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnBeforeInvocation(null);
		}
		
		protected override void AfterTest()
		{
			base.AfterTest();
			Resolve<IMessageBrokerUnitOfWorkAspect>().OnAfterInvocation(null, null);
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}