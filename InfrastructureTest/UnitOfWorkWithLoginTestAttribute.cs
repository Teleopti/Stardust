using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class UnitOfWorkWithLoginTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWork unitOfWork;
		private SetupFixtureForAssembly.TestScope _loginWithOpenUnitOfWork;

		protected override void BeforeTest()
		{
			IPerson loggedOnPerson;
			_loginWithOpenUnitOfWork = SetupFixtureForAssembly.CreatePersonAndLoginWithOpenUnitOfWork(out loggedOnPerson, out unitOfWork);
		}

		protected override void AfterTest()
		{
			_loginWithOpenUnitOfWork.Teardown();
		}
	}
}