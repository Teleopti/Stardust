using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class UnitOfWorkWithLoginTestAttribute : InfrastructureTestAttribute
	{
		private SetupFixtureForAssembly.TestScope _loginWithOpenUnitOfWork;

		protected override void BeforeTest()
		{
			_loginWithOpenUnitOfWork = SetupFixtureForAssembly.CreatePersonAndLoginWithOpenUnitOfWork(out _, out _);
		}

		protected override void AfterTest()
		{
			_loginWithOpenUnitOfWork.Teardown();
		}
	}
}