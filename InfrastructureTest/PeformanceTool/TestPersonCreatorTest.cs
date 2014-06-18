using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.PeformanceTool;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.PeformanceTool
{
	[Category("LongRunning")]
	public class TestPersonCreatorTest : DatabaseTestWithoutTransaction
	{
		private static TestPersonCreator createTarget(IUnitOfWork uow, IPersonRepository personRepository)
		{
			return new TestPersonCreator(personRepository,
				new TeamRepository(uow),
				new SiteRepository(uow),
				new PartTimePercentageRepository(uow),
				new ContractRepository(uow),
				new ContractScheduleRepository(uow),
				new ExternalLogOnRepository(uow));
		}

		[Test]
		public void ShouldCreateMultiplePersons()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personRepository = new PersonRepository(uow);
				var target = createTarget(uow, personRepository);

				target.CreatePersons(5);
				uow.PersistAll();

				personRepository.LoadAll().Count.Should().Be(5);

				cleanup(target, uow);
			}
		}

		[Test]
		public void ShouldCreateExternalLogOnForCreatedPerson()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personRepository = new PersonRepository(uow);
				var target = createTarget(uow, personRepository);

				target.CreatePersons(1);
				uow.PersistAll();

				var person = personRepository.LoadAll().Single();
				var externaLogOnString = person.PersonPeriodCollection.Last().ExternalLogOnCollection.First().AcdLogOnName; 
				externaLogOnString.Should().Not.Be.NullOrEmpty();

				cleanup(target, uow);
			}	
		}

		private static void cleanup(ITestPersonCreator target, IUnitOfWork uow)
		{
			target.RemoveCreatedPersons();
			uow.PersistAll();
		}
	}
}
