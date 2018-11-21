using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[DatabaseTest]
	public class DayOffOptimizationChildSkillTest
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
		public IJobResultRepository JobResultRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IWorkloadRepository WorkloadRepository;
		public ISkillDayRepository SkillDayRepository;

		[Test]
		public void ShouldDoDayOffOptimizationForPlanningPeriodWithOneOfMultipleChildSkills()
		{
			var planningPeriod = fillDatabaseWithEnoughDataToRunScheduling();
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
		}

		private PlanningPeriod fillDatabaseWithEnoughDataToRunScheduling()
		{
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(1, 1, 1, 1, 1), new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCategory)) { Description = new Description("_") };
			var ruleSetBag = new RuleSetBag(ruleSet) { Description = new Description("_") };
			var date = new DateOnly(2017, 6, 1);
			var multisiteSkill = new MultisiteSkill("multi", "multi", Color.AliceBlue, 15, new SkillTypePhone(new Description("_"), ForecastSource.InboundTelephony));
			var childSkill1 = new ChildSkill("child","child", Color.AliceBlue, multisiteSkill);
			var childSkill2 = new ChildSkill("child2","child2", Color.AliceBlue, multisiteSkill);
			multisiteSkill.IsOpen().For(activity);
			var team = new Team {Site = new Site("_")};
			team.SetDescription(new Description("_"));
			var agent1 = new Person()
				.WithPersonPeriod(ruleSetBag, new Contract("_"), team, childSkill1).InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneWeek(date);
			var agent2 = new Person()
				.WithPersonPeriod(ruleSetBag, new Contract("_"), team, childSkill2).InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneWeek(date);
			var period = date.ToDateOnlyPeriod();
			var planningGroup = new PlanningGroup();
			planningGroup.AddFilter(new SkillFilter(childSkill1));
			var planningPeriod = new PlanningPeriod(date, SchedulePeriodType.Day, 1, planningGroup);
			var skillDay = multisiteSkill.CreateSkillDayWithDemand(scenario, date, 1);
			
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")));
				SiteRepository.Add(agent1.Period(date).Team.Site);
				TeamRepository.Add(agent1.Period(date).Team);
				PartTimePercentageRepository.Add(agent1.Period(date).PersonContract.PartTimePercentage);
				PartTimePercentageRepository.Add(agent2.Period(date).PersonContract.PartTimePercentage);
				ContractRepository.Add(agent1.Period(date).PersonContract.Contract);
				ContractRepository.Add(agent2.Period(date).PersonContract.Contract);
				ContractScheduleRepository.Add(agent1.Period(date).PersonContract.ContractSchedule);
				ContractScheduleRepository.Add(agent2.Period(date).PersonContract.ContractSchedule);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(multisiteSkill.SkillType);
				SkillRepository.Add(multisiteSkill);
				WorkloadRepository.AddRange(multisiteSkill.WorkloadCollection);
				ShiftCategoryRepository.Add(shiftCategory);
				SkillRepository.Add(childSkill1);
				SkillRepository.Add(childSkill2);
				WorkShiftRuleSetRepository.Add(ruleSetBag.RuleSetCollection.Single());
				RuleSetBagRepository.Add(ruleSetBag);
				PersonRepository.Add(agent1);
				PersonRepository.Add(agent2);
				var jobResult = new JobResult(JobCategory.WebSchedule, period, agent1, DateTime.UtcNow);
				JobResultRepository.Add(jobResult);
				PlanningGroupRepository.Add(planningGroup);
				PlanningPeriodRepository.Add(planningPeriod);
				SkillDayRepository.Add(skillDay);
				uow.PersistAll();
			}
			return planningPeriod;
		}
	}
}