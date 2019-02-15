using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class UnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWorkAspect _aspect;

		protected override void BeforeTest()
		{
			InfrastructureTestSetup.Before();
			base.BeforeTest();
			_aspect = Resolve<IUnitOfWorkAspect>();
			_aspect.OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			_aspect?.OnAfterInvocation(null, null);
			_aspect = null;
			InfrastructureTestSetup.After();
		}
	}
}