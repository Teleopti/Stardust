using System;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly VirtualSkillContext _virtualSkillContext;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper,
																Func<ISchedulerStateHolder> stateHolder,
																VirtualSkillContext virtualSkillContext)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
			_virtualSkillContext = virtualSkillContext;
		}

		public void ForDay(DateOnly date)
		{
			using (_virtualSkillContext.Create(new DateOnlyPeriod(date, date)))
			{
				//TODO - we don't want to do this for every day, need a DateOnlyPeriod method as well?
				using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider()).Create())
				{
					_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //ska vara true, true - fixa och lägg på test senare

					var schedulingResult = _stateHolder().SchedulingResultState;
					var cascadingSkills = schedulingResult.CascadingSkills().ToArray();
					//TODO: hantera deletade skills?? (kanske inte behövs här)
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
								remainingOverstaff -= resourcesToMove;
								skillStaffPeriodTo.SetCalculatedResource65(skillStaffPeriodTo.CalculatedResource + resourcesToMove);
								skillStaffPeriodFrom.SetCalculatedResource65(skillStaffPeriodFrom.CalculatedResource - resourcesToMove);

								if (Math.Abs(remainingOverstaff) < 0.0000000000000001d)
									break;
							}
						}
					}
				}
			}
		}
	}
}