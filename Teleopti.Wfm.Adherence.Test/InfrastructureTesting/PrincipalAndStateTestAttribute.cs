using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class PrincipalAndStateTestAttribute : InfrastructureTestAttribute, IPrincipalAndStateContext
	{
		private IPerson person;
//		private IDisposable _login;

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService(this);
		}

		protected override void BeforeTest()
		{
			InfrastructureTestStuff.BeforeWithLogon(out person);
			base.BeforeTest();

//			_login = SetupFixtureForAssembly.CreatePersonAndLogin(out person);
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			InfrastructureTestStuff.AfterWithLogon();
//			_login?.Dispose();
//			_login = null;
		}

		public void Login()
		{
			InfrastructureTestStuff.Login(person);
//			SetupFixtureForAssembly.Login(person);
		}

		public void Logout()
		{
			InfrastructureTestStuff.Logout();
//			SetupFixtureForAssembly.Logout();
		}
	}

	public interface IPrincipalAndStateContext
	{
		void Login();
		void Logout();
	}
}