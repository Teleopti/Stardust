using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[DomainTest]
	public class RestrictionCheckerTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public ICheckerRestriction Target;
		
		[Test]
		public void ShouldReturnUnspecifedForEmptyDayWithDayOffPreference_ForDisplay()
		{
			var date = new DateOnly(2015, 10, 12); 
			var agent = new Person().WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill().For(activity).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var scheduleDatas = new List<IPersistableScheduleData>();
			var preferenceRestriction = new PreferenceRestriction {DayOffTemplate = new DayOffTemplate()};
			scheduleDatas.Add(new PreferenceDay(agent, date, preferenceRestriction));
			var stateHolder = SchedulerStateHolder.Fill(scenario, date, new[] { agent }, scheduleDatas, skillDay);
			var permissionState = Target.CheckPreferenceDayOffForDisplay(stateHolder.Schedules[agent].ScheduledDay(date));

			Assert.AreEqual(PermissionState.Unspecified, permissionState);	
		}
		
		[Test]
		public void ShouldReturnSatisfiedForDayOffWithDayOffPreference_ForDisplay()
		{
			var date = new DateOnly(2015, 10, 12); 
			var agent = new Person().WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill().For(activity).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var scheduleDatas = new List<IPersistableScheduleData>();
			var dayOff = new DayOffTemplate();
			var preferenceRestriction = new PreferenceRestriction {DayOffTemplate = dayOff};
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_"));
			ass.SetDayOff(dayOff);
			scheduleDatas.Add(new PreferenceDay(agent, date, preferenceRestriction));
			scheduleDatas.Add(ass);
			var stateHolder = SchedulerStateHolder.Fill(scenario, date, new[] { agent }, scheduleDatas, skillDay);
			var permissionState = Target.CheckPreferenceDayOffForDisplay(stateHolder.Schedules[agent].ScheduledDay(date));

			Assert.AreEqual(PermissionState.Satisfied, permissionState);	
		}
		
		[Test]
		public void ShouldReturnNoneWhenNoDayOffPreference_ForDisplay()
		{
			var date = new DateOnly(2015, 10, 12); 
			var agent = new Person().WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill().For(activity).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var scheduleDatas = new List<IPersistableScheduleData>();
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(8, 16));
			scheduleDatas.Add(ass);
			var stateHolder = SchedulerStateHolder.Fill(scenario, date, new[] { agent }, scheduleDatas, skillDay);
			var permissionState = Target.CheckPreferenceDayOffForDisplay(stateHolder.Schedules[agent].ScheduledDay(date));

			Assert.AreEqual(PermissionState.None, permissionState);	
		}
		
		[Test]
		public void ShouldReturnBrokenForMainShiftAndDayOffPreferencePreference_ForDisplay()
		{
			var date = new DateOnly(2015, 10, 12); 
			var agent = new Person().WithId();
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var skill = new Skill().For(activity).IsOpen();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);
			var scheduleDatas = new List<IPersistableScheduleData>();
			var preferenceRestriction = new PreferenceRestriction {DayOffTemplate = new DayOffTemplate()};
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory("_")).WithLayer(activity, new TimePeriod(8, 16));
			scheduleDatas.Add(new PreferenceDay(agent, date, preferenceRestriction));
			scheduleDatas.Add(ass);
			var stateHolder = SchedulerStateHolder.Fill(scenario, date, new[] { agent }, scheduleDatas, skillDay);
			var permissionState = Target.CheckPreferenceDayOffForDisplay(stateHolder.Schedules[agent].ScheduledDay(date));

			Assert.AreEqual(PermissionState.Broken, permissionState);
		}
	}
}