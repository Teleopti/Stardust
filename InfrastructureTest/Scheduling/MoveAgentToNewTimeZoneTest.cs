using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
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
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Scheduling
{
	[InfrastructureTest]
	[UseIocForFatClient]
	public class MoveAgentToNewTimeZoneDesktopTest : ISetup
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
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, date, 1, 1);
			var agent = new Person()
				.WithPersonPeriod(ruleSetBag, contract, skill).InTimeZone(TimeZoneInfo.Utc)
				.WithSchedulePeriodOneDay(date);
			const int startHour = 2;
			var ass1 = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(startHour, startHour + 8)).ShiftCategory(shiftCategory);
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				WorkloadRepository.AddRange(skill.WorkloadCollection);
				SkillDayRepository.AddRange(skillDays);
				SiteRepository.Add(agent.Period(date).Team.Site);
				TeamRepository.Add(agent.Period(date).Team);
				PartTimePercentageRepository.Add(agent.Period(date).PersonContract.PartTimePercentage);
				ContractRepository.Add(agent.Period(date).PersonContract.Contract);
				ContractScheduleRepository.Add(agent.Period(date).PersonContract.ContractSchedule);
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.AddRange(agent.Period(date).RuleSetBag.RuleSetCollection);
				RuleSetBagRepository.Add(agent.Period(date).RuleSetBag);
				PersonRepository.Add(agent);
				PersonAssignmentRepository.Add(ass1);
				uow.PersistAll();
			}
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.DenverTimeZoneInfo());
				PersonRepository.Add(agent);
				uow.PersistAll();
			}
			ISchedulerStateHolder stateHolder;
			using (UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				stateHolder = Fill(SchedulerStateHolder, scenario, period,
					PersonRepository.FindPeopleInOrganization(period, true), 
					PersonAssignmentRepository.Find(period, scenario), 
					SkillDayRepository.FindRange(period, skill, scenario));
				stateHolder.Schedules.TakeSnapshot();
			}

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);

			Assert.DoesNotThrow(() =>
			{
				Persister.Persist(stateHolder.Schedules);
			});
		}

		//flytta
		public static ISchedulerStateHolder Fill(Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> persistableScheduleData,
			IEnumerable<ISkillDay> skillDays)
		{
			var stateHolder = stateHolderFunc();
			stateHolder.SetRequestedScenario(scenario);
			stateHolder.SchedulingResultState.Schedules = WithData(scenario, period, persistableScheduleData, agents);
			foreach (var agent in agents)
			{
				stateHolder.AllPermittedPersons.Add(agent);
				stateHolder.SchedulingResultState.PersonsInOrganization.Add(agent);
			}
			stateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			var uniqueSkills = new HashSet<ISkill>();
			foreach (var skillDay in skillDays)
			{
				uniqueSkills.Add(skillDay.Skill);
			}
			stateHolder.SchedulingResultState.AddSkills(uniqueSkills.ToArray());
			foreach (var uniqueSkill in uniqueSkills)
			{
				stateHolder.SchedulingResultState.SkillDays[uniqueSkill] = skillDays.Where(skillDay => skillDay.Skill.Equals(uniqueSkill));
			}

			stateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, TimeZoneInfo.Utc);
			stateHolder.CommonStateHolder.SetDayOffTemplate(DayOffFactory.CreateDayOff());
			return stateHolder;
		}


		public static IScheduleDictionary WithData(IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IScheduleData> persistableScheduleData,
			IEnumerable<IPerson> agents)
		{
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			var ret = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(dateTimePeriod, agents), new PersistableScheduleDataPermissionChecker());
			foreach (var scheduleData in persistableScheduleData)
			{
				((ScheduleRange)ret[scheduleData.Person]).Add(scheduleData);
			}
			return ret;
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			//system.UseTestDouble<ScheduleRangePersister>().For<IScheduleRangePersister>();
		}
	}
}