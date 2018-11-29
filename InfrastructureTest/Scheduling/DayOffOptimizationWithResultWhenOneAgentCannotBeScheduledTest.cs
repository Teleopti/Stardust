using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[DatabaseTest]
	public class DayOffOptimizationWithResultWhenOneAgentCannotBeScheduledTest : IIsolateSystem
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
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public ISkillDayRepository SkillDayRepository;
		public IWorkloadRepository WorkloadRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PersistableScheduleDataPermissionChecker>().For<IPersistableScheduleDataPermissionChecker>();
		}

		[Test]
		public void ShouldNotThrowOnSkillTypeLazyLoading()
		{
			var planningPeriod = fillDatabaseWithEnoughDataToRunScheduling();
			Target.DoSchedulingAndDO(planningPeriod.Id.Value);
		}

		private PlanningPeriod fillDatabaseWithEnoughDataToRunScheduling()
		{
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 16, 0, 60), new TimePeriodWithSegment(8, 0, 16, 0, 60), shiftCategory)) { Description = new Description("_") };
			var ruleSetBag = new RuleSetBag(ruleSet) { Description = new Description("_") };
			var date = new DateOnly(2017, 6, 1);
			var skill = new Skill().IsOpen().For(activity);
			skill.SkillType.Description = new Description("_");
			var oldSkill = new Skill().IsOpen().For(activity);
			oldSkill.SkillType.Description = new Description("_");
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var team = new Team {Site = new Site("_")};
			team.SetDescription(new Description("_"));
			var contract = new Contract("_");
			var contractSchedule = new ContractSchedule("_");
			var partTimePercentage = new PartTimePercentage("_");
			var agent = new Person()
				.WithPersonPeriod(ruleSetBag, contract, contractSchedule, partTimePercentage, team, skill)
				.InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneDay(date);
			var agentWithNoSchedulePeriod = new Person()
				.WithPersonPeriod(ruleSetBag, contract, contractSchedule, partTimePercentage, team, oldSkill)
				.InTimeZone(TimeZoneInfo.Utc);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var ass2 = new PersonAssignment(agentWithNoSchedulePeriod, scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var planningGroup = new PlanningGroup();
			var planningPeriod = new PlanningPeriod(date, SchedulePeriodType.Day, 1, planningGroup);

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")));
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(contract);
				PartTimePercentageRepository.Add(partTimePercentage);
				ContractScheduleRepository.Add(contractSchedule);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);				
				SkillTypeRepository.Add(oldSkill.SkillType);
				SkillRepository.Add(oldSkill);
				WorkloadRepository.AddRange(skill.WorkloadCollection);
				WorkloadRepository.AddRange(oldSkill.WorkloadCollection);
				SkillDayRepository.Add(skillDay);
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.Add(ruleSetBag.RuleSetCollection.Single());
				RuleSetBagRepository.Add(ruleSetBag);
				PersonRepository.Add(agent);
				PersonRepository.Add(agentWithNoSchedulePeriod);
				PersonAssignmentRepository.Add(ass);
				PersonAssignmentRepository.Add(ass2);
				PlanningGroupRepository.Add(planningGroup);
				PlanningPeriodRepository.Add(planningPeriod);
				uow.PersistAll();
			}
			return planningPeriod;
		}
	}
}