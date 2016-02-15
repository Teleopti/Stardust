using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.IslandScheduling
{
	public class IslandSchedulerStateProvider
	{
		private readonly ISchedulerStateHolder _stateholderToLoadFrom;
		private readonly IList<IPerson> _personsInIsland;
		private readonly IEnumerable<string> _islandSkillGuidStrings;

		public IslandSchedulerStateProvider(ISchedulerStateHolder stateholderToLoadFrom, IList<IPerson> personsInIsland, IEnumerable<string> islandSkillGuidStrings)
		{
			_stateholderToLoadFrom = stateholderToLoadFrom;
			_personsInIsland = personsInIsland;
			_islandSkillGuidStrings = islandSkillGuidStrings;
		}

		public ISchedulerStateHolder Load(ISchedulerStateHolder stateholderToLoad)
		{
			((IIslandSchedulerStateHolder)stateholderToLoad).SetCommonStateHolder(_stateholderToLoadFrom.CommonStateHolder);
			stateholderToLoad.SchedulingResultState.AllPersonAccounts =
				_stateholderToLoadFrom.SchedulingResultState.AllPersonAccounts;

			createScheduleDictionary(stateholderToLoad);
			loadSchedules(stateholderToLoad);
			loadSkills(stateholderToLoad);
			loadSkillData(stateholderToLoad);

			return stateholderToLoad;
		}

		private void createScheduleDictionary(ISchedulerStateHolder stateholderToLoad)
		{
			var dicToLoadFrom = _stateholderToLoadFrom.Schedules;
			stateholderToLoad.SchedulingResultState.Schedules = new ScheduleDictionary(dicToLoadFrom.Scenario, dicToLoadFrom.Period);
		}

		private void loadSchedules(ISchedulerStateHolder stateholderToLoad)
		{
			foreach (var person in _personsInIsland)
			{
				var rangeToLoadFrom = _stateholderToLoadFrom.Schedules[person];
				var allSchedules =
					rangeToLoadFrom.ScheduledDayCollection(
						rangeToLoadFrom.Period.ToDateOnlyPeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone));

				stateholderToLoad.Schedules.Modify(ScheduleModifier.Scheduler, allSchedules, NewBusinessRuleCollection.Minimum(),
					new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());
			}
		}

		private void loadSkills(ISchedulerStateHolder stateholderToLoad)
		{
			foreach (var skillGuidString in _islandSkillGuidStrings)
			{
				Guid guid;
				if (!Guid.TryParse(skillGuidString, out guid))
					continue;

				var skill = _stateholderToLoadFrom.SchedulingResultState.Skills.FirstOrDefault(s => s.Id == guid);
				if (skill == null)
					continue;

				stateholderToLoad.SchedulingResultState.AddSkills(skill);
			}
		}

		private void loadSkillData(ISchedulerStateHolder stateholderToLoad)
		{
			var relevantSkillDays = new Dictionary<ISkill, IList<ISkillDay>>();
			foreach (var skill in stateholderToLoad.SchedulingResultState.Skills)
			{
				IList<ISkillDay> skilldays;
				if(!_stateholderToLoadFrom.SchedulingResultState.SkillDays.TryGetValue(skill, out skilldays))
					continue;

				relevantSkillDays.Add(skill, skilldays);
			}

			stateholderToLoad.SchedulingResultState.SkillDays = relevantSkillDays;
		}

		
	}
}