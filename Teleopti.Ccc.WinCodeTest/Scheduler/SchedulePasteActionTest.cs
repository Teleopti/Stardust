using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	public class SchedulePasteActionTest
	{
		[Test]
		public void ShouldKeepSourceShiftWhenMergingOvertimeShiftWithMainShiftLayerOnNextDay()
		{
			var target = new SchedulePasteAction(null, new GridlockManager(), SchedulePartFilter.None, new TimeZoneGuard());
			var multiplicatorSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("_", MultiplicatorType.Overtime);
			var contract = new Contract("_");
			contract.AddMultiplicatorDefinitionSetCollection(multiplicatorSet);
			var timeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();
			var agent = new Person().InTimeZone(timeZone).WithPersonPeriod(contract);
			var scenario = new Scenario();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1)), new Dictionary<IPerson, IScheduleRange>());
			var activity = new Activity();
			var source = ExtractedSchedule.CreateScheduleDay(dic, agent, new DateOnly(2000,1,2), new FullPermission());
			var destination = ExtractedSchedule.CreateScheduleDay(dic, agent, new DateOnly(2000,1,1), new FullPermission());
			var nightShiftPeriodForOverTime = new DateTimePeriod(2000, 1, 2, 23, 2000, 1, 3, 7);
			var periodOnNextDay = new DateTimePeriod(2000, 1, 3, 2, 2000, 1, 3, 3);
			source.CreateAndAddOvertime(activity, nightShiftPeriodForOverTime, multiplicatorSet);
			source.CreateAndAddActivity(activity, periodOnNextDay, new ShiftCategory("_"));
			dic.Modify(source, NewBusinessRuleCollection.Minimum());

			dic[agent].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment(true).ShiftLayers.Count()
				.Should().Be.EqualTo(0);
			dic[agent].ScheduledDay(new DateOnly(2000, 1, 2)).PersonAssignment(true).ShiftLayers.Count()
				.Should().Be.EqualTo(2);
			
			target.Paste(source, destination, new PasteOptions {MainShift = true, Overtime = true});
			dic.Modify(destination, NewBusinessRuleCollection.Minimum());

			dic[agent].ScheduledDay(new DateOnly(2000, 1, 1)).PersonAssignment(true).ShiftLayers.Count()
				.Should().Be.EqualTo(2);
			dic[agent].ScheduledDay(new DateOnly(2000, 1, 2)).PersonAssignment(true).ShiftLayers.Count()
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldKeepStartHourWhenPastingOverDST([Values(29, 30)] int dateNumber)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			var target = new SchedulePasteAction(null, new GridlockManager(), SchedulePartFilter.None, new TimeZoneGuard());
			var agent = new Person().InTimeZone(timeZone);
			var scenario = new Scenario();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(2017, 1, 1, 2018, 1, 1)), new Dictionary<IPerson, IScheduleRange>());
			var sourceDST = ExtractedSchedule.CreateScheduleDay(dic, agent, new DateOnly(2017, 10, 26), new FullPermission());
			sourceDST.CreateAndAddActivity(new Activity(), new DateTimePeriod(2017, 10, 26, 8, 2017, 10, 26, 17), new ShiftCategory("_"));
			var destinationNonDST = ExtractedSchedule.CreateScheduleDay(dic, agent, new DateOnly(2017, 10, dateNumber), new FullPermission());

			target.Paste(sourceDST, destinationNonDST, new PasteOptions {Default = true});

			var sourceStart = TimeZoneHelper.ConvertFromUtc(sourceDST.PersonAssignment().Period.StartDateTime, timeZone);
			var destinationStart = TimeZoneHelper.ConvertFromUtc(destinationNonDST.PersonAssignment().Period.StartDateTime, timeZone);
			sourceStart.Hour.Should().Be.EqualTo(destinationStart.Hour);
		}
	}
}