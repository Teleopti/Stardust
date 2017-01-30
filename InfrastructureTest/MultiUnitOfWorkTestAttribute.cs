using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class MultiUnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWorkAspect _aspect;

		protected override void BeforeTest()
		{
			base.BeforeTest();

			_aspect = Resolve<IUnitOfWorkAspect>();
			_aspect.OnBeforeInvocation(null);

			Resolve<IReadModelUnitOfWorkAspect>().OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			Resolve<IReadModelUnitOfWorkAspect>().OnAfterInvocation(null, null);

			_aspect.OnAfterInvocation(null, null);
			_aspect = null;

			SetupFixtureForAssembly.RestoreAnalyticsDatabase();
			SetupFixtureForAssembly.RestoreCcc7Database();
		}
	}
}