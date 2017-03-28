using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTestWithStaticDependenciesAvoidUse]
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

			var ass2 = new PersonAssignment(agent, scenario, dateOnly.AddDays(1)); //should not alert
			ass2.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass2.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			ass2.SetShiftCategory(shiftCategory1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1),
				new[] { agent }, new[] { ass1, ass2 }, Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> {agent}, CultureInfo.InvariantCulture, bussinesRuleCollection);

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(1);
			scheduleDay.BusinessRuleResponseCollection.First().TypeOfRule.Should().Be.EqualTo(typeof (NotOverwriteLayerRule));

			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly.AddDays(1));
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);

			var scheduleDayToModify = stateHolder.Schedules.SchedulesForDay(dateOnly).First();
			scheduleDayToModify.PersonAssignment().ClearPersonalActivities();
			stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);		
		}

		[Test]
		public void ShouldHandleMoreThanOnePersonAsync()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_") { AllowOverwrite = true };
			var lunch = new Activity("_") { AllowOverwrite = false };
			var personalActivity = new Activity("_") { AllowOverwrite = true };
			var dateOnly = new DateOnly(2016, 05, 23);
			var shiftCategory1 = new ShiftCategory("_").WithId();

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var agent1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod1 = agent1.SchedulePeriod(dateOnly);
			schedulePeriod1.PeriodType = SchedulePeriodType.Week;

			var ass1 = new PersonAssignment(agent, scenario, dateOnly); //should alert
			ass1.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass1.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			var period = new TimePeriod(10, 0, 12, 0);
			var periodAsDateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dateOnly.Date.Add(period.StartTime),
				dateOnly.Date.Add(period.EndTime),
				agent.PermissionInformation.DefaultTimeZone());
			ass1.AddPersonalActivity(personalActivity, periodAsDateTimePeriod);
			ass1.SetShiftCategory(shiftCategory1);

			var ass2 = new PersonAssignment(agent, scenario, dateOnly.AddDays(1)); //should not alert
			ass2.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass2.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			ass2.SetShiftCategory(shiftCategory1);

			var ass3 = new PersonAssignment(agent1, scenario, dateOnly); //should not alert
			ass3.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass3.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			ass3.SetShiftCategory(shiftCategory1);

			var ass4 = new PersonAssignment(agent1, scenario, dateOnly.AddDays(1)); //should not alert
			ass4.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass4.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			ass4.SetShiftCategory(shiftCategory1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1),
				new[] { agent, agent1 }, new[] { ass1, ass2, ass3, ass4 }, Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> { agent }, CultureInfo.InvariantCulture, bussinesRuleCollection);

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(1);
			scheduleDay.BusinessRuleResponseCollection.First().TypeOfRule.Should().Be.EqualTo(typeof(NotOverwriteLayerRule));
			scheduleDay = stateHolder.Schedules[agent1].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);

			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly.AddDays(1));
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);
			scheduleDay = stateHolder.Schedules[agent1].ScheduledDay(dateOnly.AddDays(1));
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);

			var scheduleDayToModify = stateHolder.Schedules.SchedulesForDay(dateOnly).First();
			scheduleDayToModify.PersonAssignment().ClearPersonalActivities();
			stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);
			scheduleDay = stateHolder.Schedules[agent1].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);
		}

		[Test, SetUICulture("sv-SE")]
		public void ShouldShowValidationAlertIfOverwrittenByAnotherMainShiftLayer()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_") { AllowOverwrite = true };
			var lunch = new Activity("_") { AllowOverwrite = false };
			var dateOnly = new DateOnly(2016, 05, 23);
			var shiftCategory1 = new ShiftCategory("_").WithId();

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var ass1 = new PersonAssignment(agent, scenario, dateOnly); //should alert
			ass1.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass1.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			ass1.AddActivity(phoneActivity, new TimePeriod(11, 30, 12, 30));
			ass1.SetShiftCategory(shiftCategory1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1),
				new[] { agent }, new[] { ass1 }, Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> { agent }, CultureInfo.InvariantCulture, bussinesRuleCollection);

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(1);

			var response = scheduleDay.BusinessRuleResponseCollection.First();
			response.TypeOfRule.Should().Be.EqualTo(typeof(NotOverwriteLayerRule));
			Assert.IsTrue(response.FriendlyName.StartsWith("Icke överskrivningsbar aktivitet"));
			Assert.IsTrue(response.Message.StartsWith("Den icke överskrivningsbara"));

			var scheduleDayToModify = stateHolder.Schedules.SchedulesForDay(dateOnly).First();
			scheduleDayToModify.PersonAssignment().RemoveActivity(scheduleDayToModify.PersonAssignment().ShiftLayers.ToList()[2]);
			stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDayToModify, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotAlertIfLunchShadowingLunch()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_") { AllowOverwrite = true };
			var lunch = new Activity("_") { AllowOverwrite = false };
			var dateOnly = new DateOnly(2016, 05, 23);
			var shiftCategory1 = new ShiftCategory("_").WithId();

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var ass1 = new PersonAssignment(agent, scenario, dateOnly); //should alert
			ass1.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass1.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));
			ass1.AddActivity(lunch, new TimePeriod(11, 30, 12, 30));
			ass1.SetShiftCategory(shiftCategory1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1),
				new[] { agent }, new[] { ass1 }, Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> { agent }, CultureInfo.InvariantCulture, bussinesRuleCollection);

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);
		}
	}
}