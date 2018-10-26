using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class UnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWorkAspect _aspect;

		protected override void BeforeTest()
		{
			InfrastructureTestStuff.BeforeWithLogon();
			base.BeforeTest();
			_aspect = Resolve<IUnitOfWorkAspect>();
			_aspect.OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			_aspect?.OnAfterInvocation(null, null);
			_aspect = null;
			InfrastructureTestStuff.AfterWithLogon();
		}
	}
}