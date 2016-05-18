using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
						.Where(x => x.Activity.Equals(skillToMoveFrom.Activity) && !skillToMoveFrom.Equals(x) && VirtualSkillContext.VirtualSkillGroupResult.BelongsToSameSkillGroup(skillToMoveFrom, x)))
					{
						ISkillStaffPeriodDictionary skillStaffPeriodToDic;
						if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillToMoveTo, out skillStaffPeriodToDic))
							continue;

						var skillStaffPeriodTo = skillStaffPeriodToDic.SkillStaffPeriodOrDefault(interval);
						var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
						if (skillToMoveToAbsoluteDifference >= 0)
							continue;
						var resourcesToMove = Math.Min(Math.Abs(skillToMoveToAbsoluteDifference), remainingOverstaff);
						skillStaffPeriodTo.TakeResourcesFrom(skillStaffPeriodFrom, resourcesToMove);

						remainingOverstaff -= resourcesToMove;
						if (Math.Abs(remainingOverstaff) < 0.0000000000000001d)
							break;
					}
				}
			}
		}
	}
}