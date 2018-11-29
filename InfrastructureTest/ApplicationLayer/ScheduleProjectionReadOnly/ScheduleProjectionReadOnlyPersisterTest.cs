using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.ScheduleProjectionReadOnly
{
	[UnitOfWorkWithLoginTest]
	public class ScheduleProjectionReadOnlyPersisterTest
	{
		public IScheduleProjectionReadOnlyPersister Persister;

		public ICurrentUnitOfWork UnitOfWork;

		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IRuleSetBagRepository RuleSetBagRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IWorkloadRepository WorkloadRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IBudgetGroupRepository BudgetGroupRepository;

		[Test]
		public void ShouldAddLayer()
		{
			var scenario = Guid.NewGuid();
			var person = Guid.NewGuid();

			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = "2016-04-29".Date(),
					ScenarioId = scenario,
					PersonId = person,
					StartDateTime = "2016-04-29 8:00".Utc(),
					EndDateTime = "2016-04-29 17:00".Utc()
				});

			Persister.ForPerson("2016-04-29".Date(), person, scenario).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPersistWithProperties()
		{
			var scenario = Guid.NewGuid();
			var person = Guid.NewGuid();
			var payloadId = Guid.NewGuid();

			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = "2016-04-29".Date(),
					ScenarioId = scenario,
					PersonId = person,
					StartDateTime = "2016-04-29 8:00".Utc(),
					EndDateTime = "2016-04-29 17:00".Utc(),
					PayloadId = payloadId,
					WorkTime = "4".Hours(),
					ContractTime = "3".Hours(),
					Name = "Phone",
					ShortName = "Ph",
					DisplayColor = 182,
				});

			var model = Persister.ForPerson("2016-04-29".Date(), person, scenario).Single();
			model.BelongsToDate.Should().Be("2016-04-29".Date());
			model.ScenarioId.Should().Be(scenario);
			model.PersonId.Should().Be(person);
			model.StartDateTime.Should().Be("2016-04-29 8:00".Utc());
			model.EndDateTime.Should().Be("2016-04-29 17:00".Utc());
			model.PayloadId.Should().Be(payloadId);
			model.WorkTime.Should().Be("4".Hours());
			model.ContractTime.Should().Be("3".Hours());
			model.Name.Should().Be("Phone");
			model.ShortName.Should().Be("Ph");
			model.DisplayColor.Should().Be(182);
		}

		[Test]
		public void
			ShouldGetNumberOfAbsencesPerDayAndBudgetGroupBasedOnCustomShrinkage()

		{
			var date = new DateOnly(2018, 3, 15);
			var absences = createTwoDifferentAbsences();
			var holidayAbsence = getHolidayAbsence(absences);
			var budgetGroup = createBudgetGroupWithHolidayAbsenceAsCustomShinkage(holidayAbsence);
			var agents = createAgents(date, budgetGroup);

			createTwoHolidayAndOneIllnessAbsenceRequests(date, agents, absences);
			createScheduleProjectionReadOnlyModels(date, agents, absences);

			Assert.AreEqual(2, AbsenceRepository.LoadAll().Count());
			Assert.AreEqual(10, PersonRepository.LoadAll().Count());
			Assert.AreEqual(2, Persister.GetNumberOfAbsencesPerDayAndBudgetGroup(budgetGroup.Id.Value, date));
		}

		private List<IAbsence> createTwoDifferentAbsences()
		{
			var holidayAbsence = new Absence {Description = new Description("Holiday"), Requestable = true};
			var illnessAbsence = new Absence {Description = new Description("Illness"), Requestable = true};
			var absences = new List<IAbsence> {holidayAbsence, illnessAbsence};
			AbsenceRepository.AddRange(absences);
			UnitOfWork.Current().PersistAll();

			return absences;
		}

		private IAbsence getHolidayAbsence(List<IAbsence> absences)
		{
			return absences.Single(x => x.Description.Name == "Holiday");
		}

		private IAbsence getIllnessAbsence(List<IAbsence> absences)
		{
			return absences.Single(x => x.Description.Name == "Illness");
		}

		private IBudgetGroup createBudgetGroupWithHolidayAbsenceAsCustomShinkage(IAbsence holidayAbsence)
		{
			var customShrinkage = new CustomShrinkage("_");
			customShrinkage.AddAbsence(holidayAbsence);
			var budgetGroup = new BudgetGroup { Name = "_", TimeZone = TimeZoneInfo.Utc };
			budgetGroup.AddCustomShrinkage(customShrinkage);
			BudgetGroupRepository.Add(budgetGroup);
			UnitOfWork.Current().PersistAll();

			return budgetGroup;
		}

		private List<IPerson> createAgents(DateOnly date, IBudgetGroup budgetGroup)
		{
			var activity = new Activity("_");
			var team = new Team { Site = new Site("_") };
			team.SetDescription(new Description("_"));

			var partTimePercentage = new PartTimePercentage("_");
			var contract = new Contract("_");
			var contractSchedule = new ContractSchedule("_");

			var skill = new Skill().IsOpen().For(activity);
			skill.SkillType.Description = new Description("_");
			skill.DefaultResolution = 60;
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(1, 1, 1, 1, 1),
				new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCategory))
			{
				Description = new Description("_")
			};
			var ruleSetBag = new RuleSetBag(ruleSet) { Description = new Description("_") };

			var agents = new List<IPerson>();
			for (var i = 0; i < 10; i++)
			{
				var agent = new Person()
					.WithPersonPeriod(date, ruleSetBag, contract, contractSchedule, partTimePercentage, team, skill)
					.InTimeZone(TimeZoneInfo.Utc)
					.WithSchedulePeriodOneWeek(date);
				foreach (var personPeriod in agent.PersonPeriods(date.ToDateOnlyPeriod()))
				{
					personPeriod.BudgetGroup = budgetGroup;
				}

				agents.Add(agent);
			}

			SiteRepository.Add(team.Site);
			TeamRepository.Add(team);
			PartTimePercentageRepository.Add(partTimePercentage);
			ContractRepository.Add(contract);
			ContractScheduleRepository.Add(contractSchedule);
			ActivityRepository.Add(activity);
			SkillTypeRepository.Add(skill.SkillType);
			SkillRepository.Add(skill);
			WorkloadRepository.AddRange(skill.WorkloadCollection);
			ShiftCategoryRepository.Add(shiftCategory);
			WorkShiftRuleSetRepository.Add(ruleSetBag.RuleSetCollection.Single());
			RuleSetBagRepository.Add(ruleSetBag);
			PersonRepository.AddRange(agents);

			UnitOfWork.Current().PersistAll();

			return agents;
		}

		private void createTwoHolidayAndOneIllnessAbsenceRequests(DateOnly date, List<IPerson> agents,  List<IAbsence> absences)
		{
			var holidayAbsence = getHolidayAbsence(absences);
			var illnessAbsence = getIllnessAbsence(absences);

			var requests = new List<IPersonRequest>();
			var shortAbsencePeriod = date.ToDateTimePeriod(new TimePeriod(10, 11), TimeZoneInfo.Utc);
			var personRequest = new PersonRequest(agents[0],
				new AbsenceRequest(holidayAbsence, shortAbsencePeriod));
			requests.Add(personRequest);

			personRequest = new PersonRequest(agents[1],
				new AbsenceRequest(illnessAbsence, date.ToDateTimePeriod(TimeZoneInfo.Utc)));
			requests.Add(personRequest);

			personRequest = new PersonRequest(agents[2],
				new AbsenceRequest(holidayAbsence, date.ToDateTimePeriod(TimeZoneInfo.Utc)));
			requests.Add(personRequest);
			PersonRequestRepository.AddRange(requests);
			UnitOfWork.Current().PersistAll();
		}

		private void createScheduleProjectionReadOnlyModels(DateOnly date, List<IPerson> agents, List<IAbsence> absences)
		{
			var holidayAbsence = getHolidayAbsence(absences);
			var illnessAbsence = getIllnessAbsence(absences);

			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = date,
					PersonId = agents[0].Id.Value,
					StartDateTime = DateTime.Now,
					EndDateTime = DateTime.Now,
					PayloadId = holidayAbsence.Id.Value
				});

			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = date,
					PersonId = agents[1].Id.Value,
					StartDateTime = DateTime.Now,
					EndDateTime = DateTime.Now,
					PayloadId = illnessAbsence.Id.Value
				});

			Persister.AddActivity(
				new ScheduleProjectionReadOnlyModel
				{
					BelongsToDate = date,
					PersonId = agents[2].Id.Value,
					StartDateTime = DateTime.Now,
					EndDateTime = DateTime.Now,
					PayloadId = holidayAbsence.Id.Value
				});
		}

	}
}