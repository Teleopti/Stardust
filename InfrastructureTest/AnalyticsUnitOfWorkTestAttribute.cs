using Teleopti.Ccc.Domain.Aop;

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