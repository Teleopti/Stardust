using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.IoC;

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