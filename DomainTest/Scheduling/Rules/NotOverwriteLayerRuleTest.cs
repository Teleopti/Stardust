using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class NotOverwriteLayerRuleTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldShowValidationAlertIfIllegalOverWriteOfActivities()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_") { AllowOverwrite = true };
			var lunch = new Activity("_") {AllowOverwrite = false};
			var personalActivity = new Activity("_") { AllowOverwrite = true };
			var dateOnly = new DateOnly(2016, 05, 23);
			var shiftCategory1 = new ShiftCategory("_").WithId();

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var ass1 = new PersonAssignment(agent, scenario, dateOnly); //should alert
			ass1.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass1.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			var period = new TimePeriod(10, 0, 12, 0);
			var periodAsDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dateOnly.Date.Add(period.StartTime),
				dateOnly.Date.Add(period.EndTime),
				agent.PermissionInformation.DefaultTimeZone());
			ass1.AddPersonalActivity(personalActivity, periodAsDateTimePeriod);
			ass1.SetShiftCategory(shiftCategory1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)),
				new[] { agent }, new[] { ass1 }, Enumerable.Empty<ISkillDay>());

			var scheduleDayToModify = stateHolder.Schedules.SchedulesForDay(dateOnly).First();
			scheduleDayToModify.CreateAndAddPublicNote("_");

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			var brokenRules = stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			foreach (var businessRuleResponse in brokenRules)
			{
				bussinesRuleCollection.Remove(businessRuleResponse);
			}

			stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(1);
			scheduleDay.BusinessRuleResponseCollection.First().TypeOfRule.Should().Be.EqualTo(typeof (NotOverwriteLayerRule));

			scheduleDayToModify.PersonAssignment().ClearPersonalActivities();
			stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);
		}
	}
}