using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
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
		public IPersonAssignmentRepository PersonAssignmentRepository;

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldDoSchedulingForPlanningPeriod(bool teamScheduling)
		{
			var planningPeriod = fillDatabaseWithEnoughDataToRunScheduling(new DateOnly(2017, 12, 11), new[]{DateOnly.MinValue});
			if (teamScheduling)
			{
				var defaultOptions = SchedulingOptionsProvider.Fetch(new DayOffTemplate());
				defaultOptions.UseTeam = true;
				defaultOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag);
				SchedulingOptionsProvider.SetFromTest(planningPeriod, defaultOptions);
			}

			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			using (UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				PersonAssignmentRepository.LoadAll().Any().Should().Be.True();
			}
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ShouldNotThrowLazyInitializationExceptionWhenHavingPersonPeriodStartingLaterThanThePlanningPeriod(bool teamScheduling)
		{
			var planningPeriod = fillDatabaseWithEnoughDataToRunScheduling(new DateOnly(2017, 12, 11), new[]{DateOnly.MinValue, new DateOnly(2018, 1, 15)});
			if (teamScheduling)
			{
				var defaultOptions = SchedulingOptionsProvider.Fetch(new DayOffTemplate());
				defaultOptions.UseTeam = true;
				defaultOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag);
				SchedulingOptionsProvider.SetFromTest(planningPeriod, defaultOptions);
			}
			
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);

			using (UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				PersonAssignmentRepository.LoadAll().Any().Should().Be.True();
			}
		}

		private PlanningPeriod fillDatabaseWithEnoughDataToRunScheduling(DateOnly planningPeriodStart, IEnumerable<DateOnly> personPeriodStarts)
		{
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			var ruleSetBag = new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 115), shiftCategory)) { Description = new Description("_") }){Description = new Description("_")};
			var contract = new Contract("_");
			var partTimePercentage=new PartTimePercentage("_");
			var contractSchedule = new ContractSchedule("_");
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.SetWorkdaysExcept(DayOfWeek.Saturday, DayOfWeek.Sunday);;
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			var skill = new Skill{SkillType = new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony)}.InTimeZone(TimeZoneInfo.Utc).IsOpen().For(activity);
			skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(planningPeriodStart, 1), 1);
			var agent = new Person().WithSchedulePeriodOneWeek(planningPeriodStart);
			var team = new Team {Site = new Site("_")};
			team.SetDescription(new Description("_"));
			foreach (var periodStart in personPeriodStarts)
			{
				agent.WithPersonPeriod(periodStart, ruleSetBag, contract, contractSchedule, partTimePercentage, team, skill).InTimeZone(TimeZoneInfo.Utc);
			}
			var planningGroup = new PlanningGroup();
			var planningPeriod = new PlanningPeriod(planningPeriodStart, SchedulePeriodType.Week, 1, planningGroup);

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")));
				SiteRepository.Add(agent.Period(planningPeriodStart).Team.Site);
				TeamRepository.Add(agent.Period(planningPeriodStart).Team);
				PartTimePercentageRepository.Add(agent.Period(planningPeriodStart).PersonContract.PartTimePercentage);
				ContractRepository.Add(agent.Period(planningPeriodStart).PersonContract.Contract);
				ContractScheduleRepository.Add(agent.Period(planningPeriodStart).PersonContract.ContractSchedule);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				WorkloadRepository.Add(skill.WorkloadCollection.Single());
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.Add(agent.Period(planningPeriodStart).RuleSetBag.RuleSetCollection.Single());
				RuleSetBagRepository.Add(agent.Period(planningPeriodStart).RuleSetBag);
				PersonRepository.Add(agent);
				var jobResult = new JobResult(JobCategory.WebSchedule, DateOnlyPeriod.CreateWithNumberOfWeeks(planningPeriodStart, 1), agent, DateTime.UtcNow);
				JobResultRepository.Add(jobResult);
				PlanningGroupRepository.Add(planningGroup);
				PlanningPeriodRepository.Add(planningPeriod);
				uow.PersistAll();
			}
			return planningPeriod;
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}