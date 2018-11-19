using NUnit.Framework;
using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[DatabaseTest]
	public class FullSchedulingUowTest: IIsolateSystem
	{
		public FullScheduling Target;
		public IScenarioRepository ScenarioRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
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
		public IPlanningPeriodRepository PlanningPeriodRepository;
		public IPlanningGroupRepository PlanningGroupRepository;
		public SchedulingOptionsProvider SchedulingOptionsProvider;
		public IJobResultRepository JobResultRepository;
		public ISkillRepository SkillRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IWorkloadRepository WorkloadRepository;
		public IPersonAssignmentRepository AssignmentRepository;
		public MutableNow Now;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldDoSchedulingForPlanningPeriod(bool teamScheduling)
		{
			if (teamScheduling)
			{
				var defaultOptions = SchedulingOptionsProvider.Fetch(new DayOffTemplate());
				defaultOptions.UseTeam = true;
				defaultOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag);
				SchedulingOptionsProvider.SetFromTest(defaultOptions);
			}

			var planningPeriod = fillDatabaseWithEnoughDataToRunScheduling();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldNotThrowLazyInitializationExceptionWhenHavingPersonPeriodStartingLaterThanThePlanningPeriod(bool teamScheduling)
		{
			Now.Is(new DateTime(2018,1,11,0,0,0,DateTimeKind.Utc));
			if (teamScheduling)
			{
				var defaultOptions = SchedulingOptionsProvider.Fetch(new DayOffTemplate());
				defaultOptions.UseTeam = true;
				defaultOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag);
				SchedulingOptionsProvider.SetFromTest(defaultOptions);
			}

			var planningPeriod = fillDatabaseWithEnoughDataToRunSchedulingAndPersonPeriodStartingLaterThanThePlanningPeriod();

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
		}

		private PlanningPeriod fillDatabaseWithEnoughDataToRunSchedulingAndPersonPeriodStartingLaterThanThePlanningPeriod()
		{
			var businesUnit = new BusinessUnit("_");
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 115), shiftCategory)) { Description = new Description("_") };
			var ruleSetBag = new RuleSetBag(ruleSet) { Description = new Description("_") };
			var team = new Team();
			team.SetDescription(new Description("_"));
			team.Site = new Site("_");
			var date = new DateOnly(2017, 12, 11);
			var contract = new Contract("_");
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.Add(DayOfWeek.Monday, true);
			contractScheduleWeek.Add(DayOfWeek.Tuesday, true);
			contractScheduleWeek.Add(DayOfWeek.Wednesday, true);
			contractScheduleWeek.Add(DayOfWeek.Thursday, true);
			contractScheduleWeek.Add(DayOfWeek.Friday, true);
			contractScheduleWeek.Add(DayOfWeek.Saturday, false);
			contractScheduleWeek.Add(DayOfWeek.Sunday, false);
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			var partTimePercentage = new PartTimePercentage("_");
			var personContract = new PersonContract(contract,partTimePercentage,contractSchedule);
			var skillTypePhone = new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony);
			var skill = new Skill("_", "asdf", Color.AliceBlue, 15, skillTypePhone);
			skill.Activity = activity;
			skill.SetBusinessUnit(businesUnit);
			skill.TimeZone = TimeZoneInfo.Utc;
			var workload = new Workload(skill);
			skill.AddWorkload(workload);
			skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1);
			var agent = new Person();
			agent.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Week, 4));
			var personPeriod1 = new PersonPeriod(date, personContract, team);
			var personPeriod2 = new PersonPeriod(new DateOnly(2018,01,15), personContract, team);
			personPeriod1.AddPersonSkill(new PersonSkill(skill,new Percent(1)));
			personPeriod2.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
			agent.AddPersonPeriod(personPeriod1);
			agent.AddPersonPeriod(personPeriod2);
			agent.PersonPeriodCollection.First().RuleSetBag = ruleSetBag;
			agent.PersonPeriodCollection[1].RuleSetBag = ruleSetBag;
			agent.InTimeZone(TimeZoneInfo.Utc);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 4);
			var planningGroup = new PlanningGroup();
			var planningPeriod = new PlanningPeriod(period, SchedulePeriodType.Week, 4, planningGroup);
			var assignment = new PersonAssignment(agent,scenario, new DateOnly(2018, 01, 05));

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				BusinessUnitRepository.Add(businesUnit);

				ScenarioRepository.Add(scenario);
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")));
				SiteRepository.Add(agent.Period(date).Team.Site);
				TeamRepository.Add(agent.Period(date).Team);
				PartTimePercentageRepository.Add(agent.Period(date).PersonContract.PartTimePercentage);
				ContractRepository.Add(agent.Period(date).PersonContract.Contract);
				ContractScheduleRepository.Add(agent.Period(date).PersonContract.ContractSchedule);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skillTypePhone);
				SkillRepository.Add(skill);
				WorkloadRepository.Add(workload);
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.Add(agent.Period(date).RuleSetBag.RuleSetCollection.Single());
				RuleSetBagRepository.Add(agent.Period(date).RuleSetBag);
				PersonRepository.Add(agent);
				AssignmentRepository.Add(assignment);
				var jobResult = new JobResult(JobCategory.WebSchedule, period, agent, DateTime.UtcNow);
				JobResultRepository.Add(jobResult);
				PlanningGroupRepository.Add(planningGroup);
				PlanningPeriodRepository.Add(planningPeriod);
				uow.PersistAll();
			}
			return planningPeriod;
		}

		private PlanningPeriod fillDatabaseWithEnoughDataToRunScheduling()
		{
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(1, 1, 1, 1, 1), new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCategory)){Description = new Description("_")};
			var ruleSetBag = new RuleSetBag(ruleSet) {Description = new Description("_")};
			var team = new Team();
			team.SetDescription(new Description("_"));
			team.Site = new Site("_");
			var date = new DateOnly(2017, 6, 1);
			var agent = new Person()
				.WithPersonPeriod(ruleSetBag, null, team).InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneWeek(date);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1);
			var planningGroup = new PlanningGroup();
			var planningPeriod = new PlanningPeriod(period, SchedulePeriodType.Week, 1, planningGroup);

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")));
				SiteRepository.Add(agent.Period(date).Team.Site);
				TeamRepository.Add(agent.Period(date).Team);
				PartTimePercentageRepository.Add(agent.Period(date).PersonContract.PartTimePercentage);
				ContractRepository.Add(agent.Period(date).PersonContract.Contract);
				ContractScheduleRepository.Add(agent.Period(date).PersonContract.ContractSchedule);
				ActivityRepository.Add(activity);
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.Add(agent.Period(date).RuleSetBag.RuleSetCollection.Single());
				RuleSetBagRepository.Add(agent.Period(date).RuleSetBag);
				PersonRepository.Add(agent);
				var jobResult = new JobResult(JobCategory.WebSchedule, period, agent, DateTime.UtcNow);
				JobResultRepository.Add(jobResult);
				PlanningGroupRepository.Add(planningGroup);
				PlanningPeriodRepository.Add(planningPeriod);
				uow.PersistAll();
			}
			return planningPeriod;
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}