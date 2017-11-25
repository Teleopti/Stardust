using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Scheduling
{
	public static class FillSchedulerStateHolderWithTestData
	{
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
				IScenario scenario,
				DateOnlyPeriod period,
				IEnumerable<IPerson> agents,
				IEnumerable<IScheduleData> persistableScheduleData,
				IEnumerable<ISkillDay> skillDays,
				IEnumerable<BpoResource> bpoResources,
				TimeZoneInfo timeZone)
		{
			var stateHolder = stateHolderFunc();
			stateHolder.SetRequestedScenario(scenario);
			stateHolder.SchedulingResultState.Schedules = ScheduleDictionaryCreator.WithData(scenario, period, persistableScheduleData, agents);
			foreach (var agent in agents)
			{
				stateHolder.ChoosenAgents.Add(agent);
				stateHolder.SchedulingResultState.LoadedAgents.Add(agent);
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
			stateHolder.SchedulingResultState.BpoResources = bpoResources;
			stateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, timeZone);
			stateHolder.CommonStateHolder.SetDayOffTemplate(DayOffFactory.CreateDayOff());
			stateHolder.Schedules.TakeSnapshot();
			return stateHolder;
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> persistableScheduleData,
			IEnumerable<ISkillDay> skillDays,
			TimeZoneInfo timeZone)
			{
				return Fill(stateHolderFunc, scenario, period, agents, persistableScheduleData, skillDays, Enumerable.Empty<BpoResource>(), timeZone);
			}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> persistableScheduleData,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, period, agents, persistableScheduleData, skillDays, Enumerable.Empty<BpoResource>(), TimeZoneInfo.Utc);
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IPerson agent,
			IEnumerable<IScheduleData> persistableScheduleData,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, period, new[]{agent}, persistableScheduleData, skillDays);
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> persistableScheduleData,
			ISkillDay skillDay)
		{
			return Fill(stateHolderFunc, scenario, period, agents, persistableScheduleData, new[] { skillDay });
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> persistableScheduleData,
			ISkillDay skillDay)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), agents, persistableScheduleData, skillDay);
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IPerson agent,
			IScheduleData persistableScheduleData,
			ISkillDay skillDay)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), new []{agent}, new []{persistableScheduleData}, skillDay);
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IPerson agent,
			IScheduleData persistableScheduleData,
			ISkillDay skillDay)
		{
			return Fill(stateHolderFunc, scenario, period, new []{agent}, new []{persistableScheduleData}, skillDay);
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IPerson agent,
			IScheduleData persistableScheduleData)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), new []{agent}, new []{persistableScheduleData}, Enumerable.Empty<ISkillDay>());
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IPerson agent)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), new []{agent}, Enumerable.Empty<IScheduleData>(), Enumerable.Empty<ISkillDay>());
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IPerson agent,
			ISkillDay skillDay)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), new []{agent}, Enumerable.Empty<IScheduleData>(), skillDay);
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IEnumerable<IPerson> agents,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), agents, Enumerable.Empty<IScheduleData>(), skillDays);
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPerson> agents,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, period, agents, Enumerable.Empty<IScheduleData>(), skillDays);
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> scheduleDatas,
			ISkillDay skillDay,
			BpoResource bpoResource)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), agents, scheduleDatas, new[] {skillDay}, new[]{bpoResource},TimeZoneInfo.Utc);
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IPerson agent,
			IScheduleData scheduleData,
			ISkillDay skillDay,
			BpoResource bpoResource)
		{
			return Fill(stateHolderFunc, scenario, date, new[]{agent}, new[]{scheduleData}, skillDay, bpoResource);
		}
	}
}