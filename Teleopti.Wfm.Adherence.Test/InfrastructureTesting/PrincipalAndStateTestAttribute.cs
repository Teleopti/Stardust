using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public interface IPrincipalAndStateContext
	{
		void Login();
		void Logout();
	}

	public class PrincipalAndStateTestAttribute : InfrastructureTestAttribute, IPrincipalAndStateContext
	{
		private IPerson person;

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService(this);
		}

		protected override void BeforeTest()
		{
			InfrastructureTestSetup.BeforeWithLogon(out person);
			base.BeforeTest();
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			InfrastructureTestSetup.AfterWithLogon();
		}

		public void Login()
		{
			InfrastructureTestSetup.Login(person);
		}

		public void Logout()
		{
			InfrastructureTestSetup.Logout();
		}
	}
}