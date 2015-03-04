using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest
{
	public class UnitOfWorkTestAttribute : InfrastructureTestAttribute
	{
		private IUnitOfWork unitOfWork;

		protected override void BeforeTest()
		{
			IPerson loggedOnPerson;
			SetupFixtureForAssembly.BeforeTestWithOpenUnitOfWork(out loggedOnPerson, out unitOfWork);
		}

		protected override void AfterTest()
		{
			SetupFixtureForAssembly.AfterTestWithOpenUnitOfWork(unitOfWork, true);
		}

	}
}