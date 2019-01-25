using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	[LoggedOnAppDomain]
	public class IntradayOptimizationTeamBlockForSingleAgentTeamAndSingleDayDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderOrg;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldNotRollBackIfSingleAgentSingleDayAndPeriodValueIsNotBetter()
		{
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
			var date = new DateOnly(2014, 4, 1);
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("DY").WithId();
			var shiftCategoryAm = new ShiftCategory("AM").WithId();
			var activity = new Activity("Phone");
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpen();
			var workShiftRuleSet =new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 7, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategoryAm));
			var agent = new Person().WithPersonPeriod(workShiftRuleSet, skill).WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodOneDay(date);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, date, 0, new Tuple<TimePeriod, double>(new TimePeriod(15, 16), 1));
			var stateHolder = SchedulerStateHolderOrg.Fill(scenario, date, new [] {agent}, new[] {ass}, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShiftCategory = true }
			};

			Target.Execute(new NoSchedulingProgress(), stateHolder, new[] { agent }, date.ToDateOnlyPeriod(), optimizationPreferences,  new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().ShiftCategory
				.Should().Be.EqualTo(shiftCategoryAm);
		}
	}
}