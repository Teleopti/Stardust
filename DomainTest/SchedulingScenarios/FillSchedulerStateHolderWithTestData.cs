﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public static class FillSchedulerStateHolderWithTestData
	{
		public static ISchedulerStateHolder Fill(this Func<ISchedulerStateHolder> stateHolderFunc,
				IScenario scenario,
				DateOnlyPeriod period,
				IEnumerable<IPerson> agents,
				IEnumerable<IScheduleData> persistableScheduleData,
				IEnumerable<ISkillDay> skillDays)
		{
			var stateHolder = stateHolderFunc();
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			stateHolder.SchedulingResultState.Schedules = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(dateTimePeriod), new PersistableScheduleDataPermissionChecker());
			foreach (var agent in agents)
			{
				stateHolder.AllPermittedPersons.Add(agent);
				stateHolder.SchedulingResultState.PersonsInOrganization.Add(agent);			
			}
			foreach (var scheduleData in persistableScheduleData)
			{
				((ScheduleRange)stateHolder.Schedules[scheduleData.Person]).Add(scheduleData);
			}
			var uniqueSkills = new HashSet<ISkill>();
			stateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			foreach (var skillDay in skillDays)
			{
				uniqueSkills.Add(skillDay.Skill);
				stateHolder.SchedulingResultState.SkillDays[skillDay.Skill] = new List<ISkillDay> { skillDay };
			}
			stateHolder.SchedulingResultState.AddSkills(uniqueSkills.ToArray());

			stateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(period, TimeZoneInfo.Utc);
			((SchedulerStateHolder) stateHolder).SetLoadedPeriod_UseOnlyFromTest_ShouldProbablyBePutOnScheduleDictionaryInsteadIfNeededAtAll(dateTimePeriod);
			return stateHolder;
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
			return Fill(stateHolderFunc, scenario, new DateOnlyPeriod(date, date), agents, persistableScheduleData, skillDay);
		}
	}
}