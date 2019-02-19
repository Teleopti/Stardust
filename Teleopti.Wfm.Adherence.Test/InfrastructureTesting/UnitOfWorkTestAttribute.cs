using Autofac;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class UnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWorkAspect _aspect;
		private (IPerson Person, IBusinessUnit BusinessUnit) _data;

		protected override void BeforeInject(IComponentContext container)
		{
			_data = InfrastructureTestSetup.Before();
			base.BeforeInject(container);
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();
			base.Login(_data.Person, _data.BusinessUnit);
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