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

		public CascadeResources(Func<ISchedulerStateHolder> stateHolder)
		{
			_stateHolder = stateHolder;
		}

		public void Execute(DateOnly date)
		{
			var schedulingResult = _stateHolder().SchedulingResultState;
			var cascadingSkills = schedulingResult.CascadingSkills().ToArray();
			foreach (var skillToMoveFrom in cascadingSkills.Reverse())
			{ 
				ISkillStaffPeriodDictionary skillStaffPeriodFromDic;
				if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveFrom, out skillStaffPeriodFromDic))
					continue;
				foreach (var interval in date.ToDateTimePeriod(skillToMoveFrom.TimeZone).Intervals(TimeSpan.FromMinutes(skillToMoveFrom.DefaultResolution)))
				{
					var skillStaffPeriodFrom = skillStaffPeriodFromDic.SkillStaffPeriodOrDefault(interval);
					var remainingOverstaff = skillStaffPeriodFrom.AbsoluteDifference;
					if (remainingOverstaff <= 0)
						continue;
					foreach (var skillToMoveTo in cascadingSkills
						.Where(x => x.Activity.Equals(skillToMoveFrom.Activity) && !skillToMoveFrom.Equals(x)))
					{
						var resourcesInGroup = resourcesInSkillGroup(skillToMoveFrom.Activity, skillToMoveTo, skillToMoveFrom, interval);
						//TODO: perf: hoppa ur ifall resourceingroup är 0. fixar sen
						ISkillStaffPeriodDictionary skillStaffPeriodToDic;
						if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveTo, out skillStaffPeriodToDic))
							continue;

						var skillStaffPeriodTo = skillStaffPeriodToDic.SkillStaffPeriodOrDefault(interval);
						var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
						if (skillToMoveToAbsoluteDifference >= 0)
							continue;
						var resourcesToMove = Math.Min(Math.Min(Math.Abs(skillToMoveToAbsoluteDifference), remainingOverstaff), resourcesInGroup);
						skillStaffPeriodTo.TakeResourcesFrom(skillStaffPeriodFrom, resourcesToMove);

						remainingOverstaff -= resourcesToMove;
						if (Math.Abs(remainingOverstaff) < 0.0000000000000001d)
							break;
					}
				}
			}
		}

		private static double resourcesInSkillGroup(IActivity activity, ISkill skill1, ISkill skill2, DateTimePeriod period)
		{
			//TODO: Fix better and put it elsewhere! No need to do "full" affectedResources call
			return ResourceCalculationContext.Fetch()
				.AffectedResources(activity, period).Where(x => x.Key.Contains(skill1.Id.Value.ToString()) && x.Key.Contains(skill2.Id.Value.ToString()))
				.Sum(x => x.Value.Resource);
		}
	}
}