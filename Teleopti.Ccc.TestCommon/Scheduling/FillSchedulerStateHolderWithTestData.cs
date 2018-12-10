using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


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
				IEnumerable<ExternalStaff> bpoResources,
				TimeZoneInfo timeZone,
			ICurrentAuthorization currentAuthorization = null)
		{
			var stateHolder = stateHolderFunc();
			stateHolder.SetRequestedScenario(scenario);
			stateHolder.SchedulingResultState.Schedules = ScheduleDictionaryCreator.WithData(scenario, period, persistableScheduleData, agents, currentAuthorization);
			foreach (var agent in agents)
			{
				stateHolder.ChoosenAgents.Add(agent);
				stateHolder.SchedulingResultState.LoadedAgents.Add(agent);
			}
			stateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			var skillDayBySkill = skillDays.ToLookup(s => s.Skill);
		
			stateHolder.SchedulingResultState.AddSkills(skillDayBySkill.Select(s => s.Key).ToArray());
			foreach (var uniqueSkill in skillDayBySkill)
			{
				stateHolder.SchedulingResultState.SkillDays[uniqueSkill.Key] = skillDayBySkill[uniqueSkill.Key];
			}
			stateHolder.SchedulingResultState.ExternalStaff = bpoResources;
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
				return Fill(stateHolderFunc, scenario, period, agents, persistableScheduleData, skillDays, Enumerable.Empty<ExternalStaff>(), timeZone);
			}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> persistableScheduleData,
			IEnumerable<ISkillDay> skillDays,
			ICurrentAuthorization currentAuthorization = null)
		{
			return Fill(stateHolderFunc, scenario, period, agents, persistableScheduleData, skillDays, Enumerable.Empty<ExternalStaff>(), TimeZoneInfo.Utc, currentAuthorization);
		}
		
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IEnumerable<IPerson> agents,
			IScheduleData persistableScheduleData,
			ISkillDay skillDay,
			ICurrentAuthorization currentAuthorization)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), agents, new[]{persistableScheduleData}, new[]{skillDay}, Enumerable.Empty<ExternalStaff>(), TimeZoneInfo.Utc, currentAuthorization);
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
			DateOnly date,
			IPerson agent,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), agent, Enumerable.Empty<IScheduleData>(), skillDays);
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
			IEnumerable<IScheduleData> persistableScheduleData
			)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), agent, persistableScheduleData, Enumerable.Empty<ISkillDay>());
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IPerson agent,
			IEnumerable<IScheduleData> persistableScheduleData
		)
		{
			return Fill(stateHolderFunc, scenario, period, agent, persistableScheduleData, Enumerable.Empty<ISkillDay>());
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
			DateOnly date,
			IPerson agent,
			IScheduleData persistableScheduleData,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), new []{agent}, new []{persistableScheduleData}, skillDays);
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
			DateOnlyPeriod period,
			IPerson agent,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, period, agent, Enumerable.Empty<IScheduleData>(), skillDays);
		}
		
				
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnlyPeriod period,
			IPerson agent,
			IScheduleData scheduleData,
			IEnumerable<ISkillDay> skillDays)
		{
			return Fill(stateHolderFunc, scenario, period, agent, new[]{scheduleData}, skillDays);
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			IScenario scenario,
			DateOnly date,
			IEnumerable<IPerson> agents,
			IEnumerable<IScheduleData> scheduleDatas,
			ISkillDay skillDay,
			ExternalStaff externalStaff)
		{
			return Fill(stateHolderFunc, scenario, date.ToDateOnlyPeriod(), agents, scheduleDatas, new[] {skillDay}, new[]{externalStaff},TimeZoneInfo.Utc);
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			DateOnlyPeriod period,
			IEnumerable<IPerson> agents,
			ExternalStaff externalStaff)
		{
			return Fill(stateHolderFunc, new Scenario(), period, agents, Enumerable.Empty<IScheduleData>(), Enumerable.Empty<ISkillDay>(), new []{externalStaff}, TimeZoneInfo.Utc);
		}

		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
			DateOnly date,
			IEnumerable<IPerson> agents,
			ExternalStaff externalStaff)
		{
			return Fill(stateHolderFunc, date.ToDateOnlyPeriod(), agents, externalStaff);
		}
	}
}