﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Scenarios
{
	[LegacyTest]
	[DomainTest]
	public class WeeklyRestSolverTest
	{
		public WeeklyRestSolverExecuter Target;
		public SchedulerStateHolder SchedulerStateHolder;

		[Test]
		public void ShouldNotFailOnValueMustBePositive()
		{
			//need a week in stateholder
			var dateOnly = new DateOnly(2016, 4, 11);
			var weekPeriod = new DateOnlyPeriod(dateOnly, dateOnly.AddDays(6));
			SchedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(weekPeriod.Inflate(1), TimeZoneInfoFactory.StockholmTimeZoneInfo());
			SchedulerStateHolder.SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(SchedulerStateHolder.RequestedPeriod.Period());

			var phoneActivity = new Activity("_")
			{
				InWorkTime = true,
				InContractTime = true,
				RequiresSkill = true
			}.WithId();

			var otherActivity = new Activity("_")
			{
				InWorkTime = true,
				InContractTime = true,
			}.WithId();

			var skill =
				new Skill("_", "_", Color.Empty, 60, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = phoneActivity,
					TimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo()
				}.WithId();

			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			SchedulerStateHolder.SchedulingResultState.AddSkills(skill);
			var scenario = new Scenario("_");
			var skillDaySuBefore = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(-1), TimeSpan.FromMinutes(60));
			var skillDayMo = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(60));
			var skillDayTu = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(1), TimeSpan.FromMinutes(60));
			var skillDayWe = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(2), TimeSpan.FromMinutes(60));
			var skillDayTh = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(3), TimeSpan.FromMinutes(60));
			var skillDayFr = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(4), TimeSpan.FromMinutes(60));
			var skillDaySa = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(5), TimeSpan.FromMinutes(60));
			var skillDaySu = skill.CreateSkillDayWithDemand(scenario, dateOnly.AddDays(6), TimeSpan.FromMinutes(60));
			SchedulerStateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			SchedulerStateHolder.SchedulingResultState.SkillDays.Add(new KeyValuePair<ISkill, IEnumerable<ISkillDay>>(skill,
				new[] { skillDaySuBefore, skillDayMo, skillDayTu, skillDayWe, skillDayTh, skillDayFr, skillDaySa, skillDaySu }));


			var contract = new Contract("_")
			{
				WorkTimeDirective =
					new WorkTimeDirective(TimeSpan.FromHours(10), TimeSpan.FromHours(83), TimeSpan.FromHours(1), TimeSpan.FromHours(40))
			};

			var agent1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new[] { skill }).WithId();
			agent1.Period(dateOnly).PersonContract.Contract = contract;
			agent1.AddSchedulePeriod(new SchedulePeriod(dateOnly.AddDays(-7), SchedulePeriodType.Week, 1));
			agent1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());

			var agent2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new[] { skill }).WithId();
			agent2.Period(dateOnly).PersonContract.Contract = contract;
			agent2.AddSchedulePeriod(new SchedulePeriod(dateOnly.AddDays(-7), SchedulePeriodType.Week, 1));
			agent2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());

			SchedulerStateHolder.FilterPersons(new[] { agent1, agent2 });
			SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(agent1);
			SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Add(agent2);
			
			var shiftCategory = new ShiftCategory("_").WithId();
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(SchedulerStateHolder.RequestedPeriod.Period(), new[] { agent1, agent2 }).VisiblePeriod);
			SchedulerStateHolder.SchedulingResultState.Schedules = scheduleDictionary;

			foreach (var agent in SchedulerStateHolder.SchedulingResultState.PersonsInOrganization)
			{
				var ass = new PersonAssignment(agent, scenario, dateOnly.AddDays(-1));
				var startTimeUtc = TimeZoneHelper.ConvertToUtc(dateOnly.AddDays(-1).Date.AddHours(22).AddMinutes(0), agent.PermissionInformation.DefaultTimeZone());
				var endTimeUtc = TimeZoneHelper.ConvertToUtc(dateOnly.AddDays(-1).Date.AddHours(23).AddMinutes(0), agent.PermissionInformation.DefaultTimeZone());
				ass.AddActivity(phoneActivity, new DateTimePeriod(startTimeUtc, endTimeUtc));
				ass.SetShiftCategory(shiftCategory);
				scheduleDictionary.AddPersonAssignment(ass);

				foreach (var date in weekPeriod.DayCollection())
				{
					ass = new PersonAssignment(agent, scenario, date);
					if (date == weekPeriod.StartDate || date == weekPeriod.EndDate.AddDays(-1))
					{
						ass.SetDayOff(new DayOffTemplate(new Description("DayOff")));
						scheduleDictionary.AddPersonAssignment(ass);
					}
					else if(date == weekPeriod.EndDate)
					{
						startTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(12).AddMinutes(0), agent.PermissionInformation.DefaultTimeZone());
						endTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(20).AddMinutes(0), agent.PermissionInformation.DefaultTimeZone());
						ass.AddActivity(phoneActivity, new DateTimePeriod(startTimeUtc, endTimeUtc));
						ass.SetShiftCategory(shiftCategory);
						scheduleDictionary.AddPersonAssignment(ass);
					}
					else
					{
						startTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(13).AddMinutes(0), agent.PermissionInformation.DefaultTimeZone());
						endTimeUtc = TimeZoneHelper.ConvertToUtc(date.Date.AddHours(23).AddMinutes(0), agent.PermissionInformation.DefaultTimeZone());
						ass.AddActivity(otherActivity, new DateTimePeriod(startTimeUtc, endTimeUtc));
						ass.SetShiftCategory(shiftCategory);
						scheduleDictionary.AddPersonAssignment(ass);
					}
				}
			}

			var selectedScheduleDays = new List<IScheduleDay>();
			var selectedPeriod = new DateOnlyPeriod(dateOnly.AddDays(-1), dateOnly);
			foreach (var agent in SchedulerStateHolder.SchedulingResultState.PersonsInOrganization)
			{
				foreach (var date in selectedPeriod.DayCollection())
				{
					selectedScheduleDays.Add(scheduleDictionary[agent].ScheduledDay(date));
				}
			}

			var optimizationPref = new OptimizationPreferences();
			var dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences());
			Target.Resolve(optimizationPref, selectedPeriod, selectedScheduleDays,
				SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.ToList(), dayOffOptimizationPreferenceProvider);

			scheduleDictionary[agent1].ScheduledDay(dateOnly.AddDays(-1)).IsScheduled().Should().Be.False();
			scheduleDictionary[agent2].ScheduledDay(dateOnly.AddDays(-1)).IsScheduled().Should().Be.False();
		}
	}
}