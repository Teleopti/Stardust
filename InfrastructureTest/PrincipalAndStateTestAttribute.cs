using Autofac;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class PrincipalAndStateTestAttribute : InfrastructureTestAttribute, IPrincipalAndStateContext
	{
		private IPerson person;

		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterInstance(this).As<IPrincipalAndStateContext>();
		}

		protected override void BeforeTest()
		{
			SetupFixtureForAssembly.BeforeTest(out person);
		}

		protected override void AfterTest()
		{
			SetupFixtureForAssembly.AfterTest();
		}

		public void SetupPrincipalAndState()
		{
			SetupFixtureForAssembly.SetupFakeState(person);
		}

		public void Logout()
		{
			SetupFixtureForAssembly.Logout();
		}
	}

	public interface IPrincipalAndStateContext
	{
		void SetupPrincipalAndState();
		void Logout();
	}
}