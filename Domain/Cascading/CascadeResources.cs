using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadeResources
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly SkillGroupPerActivityProvider _skillGroupPerActivityProvider;

		public CascadeResources(Func<ISchedulerStateHolder> stateHolder, SkillGroupPerActivityProvider skillGroupPerActivityProvider)
		{
			_stateHolder = stateHolder;
			_skillGroupPerActivityProvider = skillGroupPerActivityProvider;
		}

		public void Execute(DateOnly date)
		{
			//TODO: if doing this for more than one day we might to do this initiazlie stuff (down to interval) only once. Or we wait with that...
			var stateHolder = _stateHolder();
			var cascadingSkills = stateHolder.SchedulingResultState.CascadingSkills().ToArray();
			if (!cascadingSkills.Any())
				return;

			var defaultResolution = cascadingSkills.First().DefaultResolution; //(maybe) correct...
			foreach (var activity in cascadingSkills.Select(x => x.Activity).Distinct())
			{
				foreach (var interval in date.ToDateTimePeriod(stateHolder.TimeZoneInfo).Intervals(TimeSpan.FromMinutes(defaultResolution)))
				{
					foreach (var skillGroup in _skillGroupPerActivityProvider.FetchOrdered(activity, interval))
					{
						var remainingResourcesInGroup = skillGroup.Resources;
						ISkillStaffPeriodDictionary skillStaffPeriodFromDic;
						if (!stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillGroup.PrimarySkill, out skillStaffPeriodFromDic))
							continue;
						var skillStaffPeriodFrom = skillStaffPeriodFromDic.SkillStaffPeriodOrDefault(interval);
						var remainingOverstaff = skillStaffPeriodFrom.AbsoluteDifference;
						if (!remainingOverstaff.IsOverstaffed())
							continue;
						foreach (var skillToMoveTo in skillGroup.CascadingSkills)
						{
							ISkillStaffPeriodDictionary skillStaffPeriodToDic;
							if (!stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveTo, out skillStaffPeriodToDic))
								continue;
							var skillStaffPeriodTo = skillStaffPeriodToDic.SkillStaffPeriodOrDefault(interval);
							var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
							if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
								continue;
							var resourcesToMove = Math.Min(Math.Min(Math.Abs(skillToMoveToAbsoluteDifference), remainingOverstaff), remainingResourcesInGroup);
							skillStaffPeriodTo.TakeResourcesFrom(skillStaffPeriodFrom, resourcesToMove);
							remainingResourcesInGroup -= resourcesToMove;
							remainingOverstaff -= resourcesToMove;
							if (remainingOverstaff.IsZero() || remainingResourcesInGroup.IsZero())
								break;
						}
					}
				}
			}
		}
	}
}