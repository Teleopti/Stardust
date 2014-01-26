using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.PerformanceTool;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.PerformanceTool
{
	[Category("LongRunning")]
	[TestFixture]
	public class PersonGeneratorTest : DatabaseTest
	{
		[Test]
		public void ShouldGeneratePersonAccordingToConfig()
		{
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var personRepository = new PersonRepository(unitOfWork);
			var siteRepository = new SiteRepository(unitOfWork);
			var teamRepository = new TeamRepository(unitOfWork);
			var partTimePercentageRepository = new PartTimePercentageRepository(unitOfWork);
			var contractRepository = new ContractRepository(unitOfWork);
			var contractScheduleRepository = new ContractScheduleRepository(unitOfWork);
			var externalLogOnRepository = new ExternalLogOnRepository(unitOfWork);
			var scheduleGenerator = MockRepository.GenerateMock<IScheduleGenerator>();
			var target = new PersonGenerator(unitOfWork, personRepository, siteRepository, teamRepository,
				partTimePercentageRepository, contractRepository, contractScheduleRepository, externalLogOnRepository,
				scheduleGenerator, new Now());
			target.Generate(1);
			var persons = personRepository.LoadAll();
			persons.Count.Should().Be(1);
			var personPeriod = persons.Single().PersonPeriodCollection.Single();
			personPeriod.Team.Should().Not.Be.Null();
			personPeriod.ExternalLogOnCollection.Single().AcdLogOnName.Should().Be("0");
		}

		[Test]
		public void ShouldGeneratePersonsAccordingToConfig()
		{
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var personRepository = new PersonRepository(unitOfWork);
			var siteRepository = new SiteRepository(unitOfWork);
			var teamRepository = new TeamRepository(unitOfWork);
			var partTimePercentageRepository = new PartTimePercentageRepository(unitOfWork);
			var contractRepository = new ContractRepository(unitOfWork);
			var contractScheduleRepository = new ContractScheduleRepository(unitOfWork);
			var externalLogOnRepository = new ExternalLogOnRepository(unitOfWork);
			var scheduleGenerator = MockRepository.GenerateMock<IScheduleGenerator>();
			var target = new PersonGenerator(unitOfWork, personRepository, siteRepository, teamRepository,
				partTimePercentageRepository, contractRepository, contractScheduleRepository, externalLogOnRepository,
				scheduleGenerator, new Now());
			target.Generate(2);
			var persons = personRepository.LoadAll();
			persons.Count.Should().Be(2);
			var personPeriod1 = persons[0].PersonPeriodCollection.Single();
			var personPeriod2 = persons[1].PersonPeriodCollection.Single();
			personPeriod1.Team.Should().Be(personPeriod2.Team);
			personPeriod1.ExternalLogOnCollection.Single().AcdLogOnName.Should().Be("0");
			personPeriod2.ExternalLogOnCollection.Single().AcdLogOnName.Should().Be("1");
		}

		[Test]
		public void ShouldReturnAllGeneratedPersonData()
		{
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var personRepository = new PersonRepository(unitOfWork);
			var siteRepository = new SiteRepository(unitOfWork);
			var teamRepository = new TeamRepository(unitOfWork);
			var partTimePercentageRepository = new PartTimePercentageRepository(unitOfWork);
			var contractRepository = new ContractRepository(unitOfWork);
			var contractScheduleRepository = new ContractScheduleRepository(unitOfWork);
			var externalLogOnRepository = new ExternalLogOnRepository(unitOfWork);
			var scheduleGenerator = MockRepository.GenerateMock<IScheduleGenerator>();
			var target = new PersonGenerator(unitOfWork, personRepository, siteRepository, teamRepository,
				partTimePercentageRepository, contractRepository, contractScheduleRepository, externalLogOnRepository,
				scheduleGenerator, new Now());
			var personData = target.Generate(1);
			personData.Persons.Single().ExternalLogOn.Should().Be("0");
		}

		[Test]
		public void ShouldReturnTeamId()
		{
			var unitOfWork = new CurrentUnitOfWork(new CurrentUnitOfWorkFactory(new CurrentTeleoptiPrincipal()));
			var personRepository = new PersonRepository(unitOfWork);
			var siteRepository = new SiteRepository(unitOfWork);
			var teamRepository = new TeamRepository(unitOfWork);
			var partTimePercentageRepository = new PartTimePercentageRepository(unitOfWork);
			var contractRepository = new ContractRepository(unitOfWork);
			var contractScheduleRepository = new ContractScheduleRepository(unitOfWork);
			var externalLogOnRepository = new ExternalLogOnRepository(unitOfWork);
			var scheduleGenerator = MockRepository.GenerateMock<IScheduleGenerator>();
			var target = new PersonGenerator(unitOfWork, personRepository, siteRepository, teamRepository,
				partTimePercentageRepository, contractRepository, contractScheduleRepository, externalLogOnRepository,
				scheduleGenerator, new Now());
			var personData = target.Generate(1);
			personData.TeamId.Should().Not.Be.Null();
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personRepository = new PersonRepository(uow);
				personRepository.LoadAll().ForEach(personRepository.Remove);
				var siteRepository = new SiteRepository(uow);
				siteRepository.LoadAll().ForEach(siteRepository.Remove);
				var teamRepository = new TeamRepository(uow);
				teamRepository.LoadAll().ForEach(teamRepository.Remove);
				var partTimePercentageRepository = new PartTimePercentageRepository(uow);
				partTimePercentageRepository.LoadAll().ForEach(partTimePercentageRepository.Remove);
				var contractRepository = new ContractRepository(uow);
				contractRepository.LoadAll().ForEach(contractRepository.Remove);
				var contractScheduleRepository = new ContractScheduleRepository(uow);
				contractScheduleRepository.LoadAll().ForEach(contractScheduleRepository.Remove);
				var externalLogOnRepository = new ExternalLogOnRepository(uow);
				externalLogOnRepository.LoadAll().ForEach(externalLogOnRepository.Remove);

				uow.PersistAll();
			}
		}
	}
}
