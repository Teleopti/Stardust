using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[InfrastructureTest]
	[UseIocForFatClient]
	public class MoveAgentToNewTimeZoneDesktopTest
	{
		public DesktopScheduling Target;
		public IScenarioRepository ScenarioRepository;
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
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IWorkloadRepository WorkloadRepository;
		public ISkillDayRepository SkillDayRepository;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public IScheduleDictionaryPersister Persister;

		[Test]
		[Ignore("#45727 - 2 be cont")]
		public void ShouldNotCrashWhenAgentHasChangedTimeZone()
		{
			var scenario = new Scenario { DefaultScenario = true };
			var activity = new Activity("_");
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory)) { Description = new Description("_") };
			var ruleSetBag = new RuleSetBag(ruleSet) { Description = new Description("_") };
			var date = new DateOnly(2017, 6, 1);
			var period = new DateOnlyPeriod(date, date.AddDays(1));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(168), TimeSpan.FromHours(1), TimeSpan.FromHours(1))};
			var skill = new Skill().IsOpen().For(activity);
			skill.SkillType.Description = new Description("_");
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var agent = new Person()
				.WithPersonPeriod(ruleSetBag, contract, skill).InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneDay(date);
			const int startHour = 2;
			var ass1 = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(startHour, startHour + 8)).ShiftCategory(shiftCategory);
			persistSetupData(scenario, activity, skill, skillDay, agent, date, shiftCategory, ass1);
			agentsTimezoneChanged(agent);
			var stateHolder = fillSchedulersStateHolder(scenario, period, agent, ass1, skillDay);

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);

			Assert.DoesNotThrow(() =>
			{
				Persister.Persist(stateHolder.Schedules);
			});
		}

		private ISchedulerStateHolder fillSchedulersStateHolder(Scenario scenario, DateOnlyPeriod period, Person agent, IPersonAssignment ass1, ISkillDay skillDay)
		{
			//would be much better if using same logic as schedulingscreen but... 100s of rows in schedulingscreen ATM :(
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] {agent}, new[] {ass1}, skillDay);
			stateHolder.Schedules.TakeSnapshot(); //let's see if this could be put in Fill method instead
			return stateHolder;
		}

		private void agentsTimezoneChanged(Person agent)
		{
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
				PersonRepository.Add(agent);
				uow.PersistAll();
			}
		}

		private void persistSetupData(Scenario scenario, Activity activity, ISkill skill, ISkillDay skillDay, Person agent, DateOnly date, ShiftCategory shiftCategory, IPersonAssignment ass)
		{
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				WorkloadRepository.AddRange(skill.WorkloadCollection);
				SkillDayRepository.Add(skillDay);
				SiteRepository.Add(agent.Period(date).Team.Site);
				TeamRepository.Add(agent.Period(date).Team);
				PartTimePercentageRepository.Add(agent.Period(date).PersonContract.PartTimePercentage);
				ContractRepository.Add(agent.Period(date).PersonContract.Contract);
				ContractScheduleRepository.Add(agent.Period(date).PersonContract.ContractSchedule);
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.AddRange(agent.Period(date).RuleSetBag.RuleSetCollection);
				RuleSetBagRepository.Add(agent.Period(date).RuleSetBag);
				PersonRepository.Add(agent);
				PersonAssignmentRepository.Add(ass);
				uow.PersistAll();
			}
		}
	}
}