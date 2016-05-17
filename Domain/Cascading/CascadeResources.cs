using System;
using System.Linq;
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
			foreach (var skillToMoveFrom in cascadingSkills)
			{
				var skillStaffPeriodFromDic = schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skillToMoveFrom];
				foreach (var interval in date.ToDateTimePeriod(skillToMoveFrom.TimeZone).Intervals(TimeSpan.FromMinutes(skillToMoveFrom.DefaultResolution)))
				{
					ISkillStaffPeriod skillStaffPeriodFrom;
					if (!skillStaffPeriodFromDic.TryGetValue(interval, out skillStaffPeriodFrom))
						continue;
					var remainingOverstaff = skillStaffPeriodFrom.AbsoluteDifference;
					if (remainingOverstaff <= 0)
						continue;
					foreach (var skillToMoveTo in cascadingSkills.Where(x => x.Activity.Equals(skillToMoveFrom.Activity) && VirtualSkillContext.VirtualSkillGroupResult.BelongsToSameSkillGroup(skillToMoveFrom, x)))
					{
						ISkillStaffPeriod skillStaffPeriodTo;
						if (!schedulingResult.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skillToMoveTo].TryGetValue(interval, out skillStaffPeriodTo))
							continue;
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