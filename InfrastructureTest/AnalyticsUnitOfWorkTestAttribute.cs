using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Infrastructure.Analytics;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class AnalyticsUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			base.BeforeTest();
			Resolve<IAnalyticsUnitOfWorkAspect>().OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			Resolve<IAnalyticsUnitOfWorkAspect>().OnAfterInvocation(null, null);
			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
		}
	}
}