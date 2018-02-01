using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseActivityOfMostUnderstaffedSkillAsOvertimeActivity()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity = createActivity("activity1");
			var moreCriticalUnderStaffedActivity = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill1 = createSkill("criticalUnderStaffedSkill1", null, timeZone);
			var criticalUnderStaffedSkill2 = createSkill("criticalUnderStaffedSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity, criticalUnderStaffedSkill1);
			var personSkill2 = createPersonSkill(moreCriticalUnderStaffedActivity, criticalUnderStaffedSkill2);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill1, 10d, 2d);
			setupIntradayStaffingForSkill(criticalUnderStaffedSkill2, 10d, 1d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(moreCriticalUnderStaffedActivity);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseFirstActivityOfSameUnderstaffedLevelSkillsAsOvertimeActivity()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity1 = createActivity("activity1");
			var criticalUnderStaffedActivity2 = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill1 = createSkill("criticalUnderStaffedSkill1", null, timeZone);
			var criticalUnderStaffedSkill2 = createSkill("criticalUnderStaffedSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity1, criticalUnderStaffedSkill1);
			var personSkill2 = createPersonSkill(criticalUnderStaffedActivity2, criticalUnderStaffedSkill2);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill1, 10d, 2d);
			setupIntradayStaffingForSkill(criticalUnderStaffedSkill2, 10d, 2d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(criticalUnderStaffedActivity1);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseActivityOfFirstMostUnderstaffedSkillAsOvertimeActivity()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity = createActivity("criticalUnderStaffedActivity");
			var moreCriticalUnderStaffedActivity1 = createActivity("moreCriticalUnderStaffedActivity1");
			var moreCriticalUnderStaffedActivity2 = createActivity("moreCriticalUnderStaffedActivity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill = createSkill("criticalUnderStaffedSkill", null, timeZone);
			var moreCriticalUnderStaffedSkill1 = createSkill("moreCriticalUnderStaffedSkill1", null, timeZone);
			var moreCriticalUnderStaffedSkill2 = createSkill("moreCriticalUnderStaffedSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity, criticalUnderStaffedSkill);
			var personSkill2 = createPersonSkill(moreCriticalUnderStaffedActivity1, moreCriticalUnderStaffedSkill1);
			var personSkill3 = createPersonSkill(moreCriticalUnderStaffedActivity2, moreCriticalUnderStaffedSkill2);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill, 10d, 2d);
			setupIntradayStaffingForSkill(moreCriticalUnderStaffedSkill1, 10d, 1d);
			setupIntradayStaffingForSkill(moreCriticalUnderStaffedSkill2, 10d, 1d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2, personSkill3);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()]
				.ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(moreCriticalUnderStaffedActivity1);
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseActivityOfMostUnderstaffedSkillAsOvertimeActivityWithinRequestPeriod()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity1 = createActivity("activity1");
			var criticalUnderStaffedActivity2 = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill1 = createSkill("criticalUnderStaffedSkill1", null, timeZone);
			var criticalUnderStaffedSkill2 = createSkill("criticalUnderStaffedSkill2", null, timeZone);

			var date = new DateOnly(2017, 7, 17);
			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity1, criticalUnderStaffedSkill1);
			var personSkill2 = createPersonSkill(criticalUnderStaffedActivity2, criticalUnderStaffedSkill2);

			var firstPeriod = new DateTimePeriod(2017, 7, 17, 11, 2017, 7, 17, 12);
			var secondPeriod = new DateTimePeriod(2017, 7, 17, 12, 2017, 7, 17, 13);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill1, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 2d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = secondPeriod},
			});

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill2, date, new List<StaffingPeriodData>
			{
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 1d, Period = firstPeriod},
				new StaffingPeriodData {ForecastedStaffing = 10d, ScheduledStaffing = 5d, Period = secondPeriod},
			});

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()]
				.ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(criticalUnderStaffedActivity2);
		}

		private void setupIntradayStaffingForSkill(ISkill skill, DateOnly date, IEnumerable<StaffingPeriodData> staffingPeriodDatas)
		{
			var skillCombinationResources = new List<SkillCombinationResource>();
			var skillForecastedStaffings = new List<Tuple<TimePeriod, double>>();

			foreach (var staffingPeriodData in staffingPeriodDatas)
			{
				skillCombinationResources.AddRange(createSkillCombinationResources(skill, staffingPeriodData.Period, staffingPeriodData.ScheduledStaffing));
				skillForecastedStaffings.AddRange(createSkillForecastedStaffings(staffingPeriodData.Period, staffingPeriodData.ForecastedStaffing));
			}

			setupIntradayStaffingForSkill(skill, date, skillCombinationResources, skillForecastedStaffings);
		}

		private void setupIntradayStaffingForSkill(ISkill skill, DateOnly date,
			IEnumerable<SkillCombinationResource> skillCombinationResources
			, IEnumerable<Tuple<TimePeriod, double>> skillForecastedStaffings)
		{
			foreach (var skillCombinationResource in skillCombinationResources)
			{
				CombinationRepository.AddSkillCombinationResource(new DateTime(),
					new[]
					{
						skillCombinationResource
					});
			}

			var skillDay = skill.CreateSkillDayWithDemandOnInterval(Scenario.Current(),
				date, 0,
				skillForecastedStaffings.ToArray());
			skillDay.SkillDataPeriodCollection.ForEach(s => { s.Shrinkage = new Percent(0.5); });
			SkillDayRepository.Has(skillDay);
		}

		private List<SkillCombinationResource> createSkillCombinationResources(ISkill skill, DateTimePeriod dateTimePeriod, double scheduledStaffing)
		{
			var skillCombinationResources = new List<SkillCombinationResource>();
			var intervals = dateTimePeriod.Intervals(TimeSpan.FromMinutes(15));
			for (var i = 0; i < intervals.Count; i++)
			{
				skillCombinationResources.Add(
					new SkillCombinationResource
					{
						StartDateTime = intervals[i].StartDateTime,
						EndDateTime = intervals[i].EndDateTime,
						Resource = scheduledStaffing,
						SkillCombination = new[] {skill.Id.Value}
					}
				);
			}
			return skillCombinationResources;
		}

		private List<Tuple<TimePeriod, double>> createSkillForecastedStaffings(DateTimePeriod dateTimePeriod, double forecastedStaffing)
		{
			var skillForecastedStaffings = new List<Tuple<TimePeriod, double>>();
			var intervals = dateTimePeriod.Intervals(TimeSpan.FromMinutes(15));
			for (var i = 0; i < intervals.Count; i++)
			{
				skillForecastedStaffings.Add(new Tuple<TimePeriod, double>(
					new TimePeriod(intervals[i].StartDateTime.TimeOfDay, intervals[i].EndDateTime.TimeOfDay),
					forecastedStaffing));
			}
			return skillForecastedStaffings;
		}

		private class StaffingPeriodData
		{
			public DateTimePeriod Period;

			public double ForecastedStaffing;

			public double ScheduledStaffing;
		}
	}
}