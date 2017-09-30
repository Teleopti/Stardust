using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingDesktopBlockTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[TestCase(true), Ignore("Bug46079")]
		[TestCase(false)]
		public void ShuldHandleBlockBetweenDaysOffWithSameShiftCatWhenMonthEndsOnTuesday(bool useBlock)
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var period = new DateOnlyPeriod(2017, 10, 1, 2017, 11, 4);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc)
				.IsOpen(new TimePeriod(8, 20));
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 12, 0, 60),
				new TimePeriodWithSegment(16, 0, 20, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromMinutes(60)));
			var contractSchedule = new ContractSchedule("realistic");
			var contractScheduleWeek = new ContractScheduleWeek();
			contractScheduleWeek.Add(DayOfWeek.Monday, true);
			contractScheduleWeek.Add(DayOfWeek.Tuesday, true);
			contractScheduleWeek.Add(DayOfWeek.Wednesday, true);
			contractScheduleWeek.Add(DayOfWeek.Thursday, true);
			contractScheduleWeek.Add(DayOfWeek.Friday, true);
			contractScheduleWeek.Add(DayOfWeek.Saturday, false);
			contractScheduleWeek.Add(DayOfWeek.Sunday, false);
			contractSchedule.AddContractScheduleWeek(contractScheduleWeek);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet, new Contract("realistic"), new Team(), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			agent.Period(period.StartDate).PersonContract.ContractSchedule = contractSchedule;

			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new []{agent}, Enumerable.Empty<IPersonAssignment>(), skillDays);

			var blockFinderType = useBlock ? BlockFinderType.BetweenDayOff : BlockFinderType.SingleDay;
			var schedulingOptions = new	SchedulingOptions{BlockFinderTypeForAdvanceScheduling = blockFinderType, BlockSameShiftCategory = true, UseBlock = true};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent},
				period);

			stateHolder.Schedules[agent].CalculatedCurrentScheduleSummary(new DateOnlyPeriod(2017, 10, 1, 2017, 10, 31))
				.ContractTime.Should().Be.EqualTo(TimeSpan.FromHours(176));
		}

		public SchedulingDesktopBlockTest(bool resourcePlannerMergeTeamblockClassicScheduling44289) : base(resourcePlannerMergeTeamblockClassicScheduling44289)
		{
		}
	}
}