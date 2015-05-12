using System;
using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class PrincipalAndStateTestAttribute : InfrastructureTestAttribute, IPrincipalAndStateContext
	{
		private IPerson person;
		private IDisposable _scope;

		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterInstance(this).As<IPrincipalAndStateContext>();
		}

		protected override void BeforeTest()
		{
			_scope = SetupFixtureForAssembly.CreatePersonAndLogin(out person);
		}

		protected override void AfterTest()
		{
			_scope.Dispose();
			_scope = null;
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