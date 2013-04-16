using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AbsenceTimeConfigurable : IPostSetup
	{
		public DateTime Date { get; set; }

		public void Apply(IPerson user, IUnitOfWork uow)
		{
			//henke: här skall vi nog hämta default?? 
			var scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
			var scenarioId = scenario.Id.GetValueOrDefault();

			var absenceRepository = new AbsenceRepository(uow);
			var absence = absenceRepository.LoadAll().First();

			var budgetGroup = new BudgetGroup { Name = "My Budget", TimeZone = TeleoptiPrincipal.Current.Regional.TimeZone };

			var shrinkage = new CustomShrinkage("test", true);
			shrinkage.AddAbsence(absence);
			budgetGroup.AddCustomShrinkage(shrinkage);

			var budgetGroupRepo = new BudgetGroupRepository(uow);
			budgetGroupRepo.Add(budgetGroup);

			var teamRepository = new TeamRepository(uow);
			var team = teamRepository.LoadAll().First();

			var partTimePercentageRepository = new PartTimePercentageRepository(uow);
			var partTimepercentage = new PartTimePercentage("some name");
			partTimePercentageRepository.Add(partTimepercentage);

			var contract = new Contract("the contract");
			var contractRepository = new ContractRepository(uow);
			contractRepository.Add(contract);

			var contractSchedule = new ContractSchedule("contract schedule");
			var contractSchedulerepository = new ContractScheduleRepository(uow);
			contractSchedulerepository.Add(contractSchedule);

			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(Date).AddDays(-1), team, budgetGroup);

			personPeriod.PersonContract.Contract = contract;
			personPeriod.PersonContract.ContractSchedule = contractSchedule;
			personPeriod.PersonContract.PartTimePercentage = partTimepercentage;

			//henke: varför behöver vi den här här??
			var person = PersonFactory.CreatePerson();
			person.AddPersonPeriod(personPeriod);

			var personRepository = new PersonRepository(uow);
			personRepository.Add(person);

			uow.PersistAll();

			var scheduleProjectionReadOnlyRepository = new ScheduleProjectionReadOnlyRepository(new FixedCurrentUnitOfWork(uow));

			var period =
				new DateOnlyPeriod(new DateOnly(Date), new DateOnly(Date)).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var layer = new DenormalizedScheduleProjectionLayer
			{
				ContractTime = TimeSpan.FromHours(8),
				WorkTime = TimeSpan.FromHours(8),
				DisplayColor = Color.Bisque.ToArgb(),
				Name = "holiday",
				ShortName = "ho",
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime,
				PayloadId = absence.Id.GetValueOrDefault()
			};

			scheduleProjectionReadOnlyRepository.AddProjectedLayer(new DateOnly(Date), scenarioId, person.Id.GetValueOrDefault(), layer);


			var usedAbsenceMinutes = TimeSpan.FromTicks(
				   scheduleProjectionReadOnlyRepository.AbsenceTimePerBudgetGroup(new DateOnlyPeriod(new DateOnly(Date).AddDays(-1), new DateOnly(Date).AddDays(1)),
													 budgetGroup, scenario).Sum(p => p.TotalContractTime)).TotalMinutes;
			Assert.That(usedAbsenceMinutes, Is.GreaterThan(0));
		}
	}
}