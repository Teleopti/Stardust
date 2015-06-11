using System;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class PrincipalAndStateTestAttribute : InfrastructureTestAttribute, IPrincipalAndStateContext
	{
		private IPerson person;
		private IDisposable _login;

		protected override void RegisterInContainer(ISystem builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.AddService(this);
		}

		protected override void BeforeTest()
		{
			_login = SetupFixtureForAssembly.CreatePersonAndLogin(out person);
		}

		protected override void AfterTest()
		{
			_login.Dispose();
			_login = null;
		}

		public void Login()
		{
			SetupFixtureForAssembly.Login(person);
		}

		public void Logout()
		{
			SetupFixtureForAssembly.Logout();
		}
	}

	public interface IPrincipalAndStateContext
	{
		void Login();
		void Logout();
	}
}