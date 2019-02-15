using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class UnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWorkAspect _aspect;

		protected override void BeforeTest()
		{
			var (person, businessUnit) = InfrastructureTestSetup.Before();
			base.BeforeTest();
			base.Login(person, businessUnit);
			_aspect = Resolve<IUnitOfWorkAspect>();
			_aspect.OnBeforeInvocation(null);
		}

		protected override void AfterTest()
		{
			_aspect?.OnAfterInvocation(null, null);
			_aspect = null;
			base.Logout();
			base.AfterTest();
			InfrastructureTestSetup.After();
		}
	}
}