using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResources
	{
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private readonly SkillGroupPerActivityProvider _skillGroupPerActivityProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ShovelResources(Func<ISchedulerStateHolder> stateHolder, 
												SkillGroupPerActivityProvider skillGroupPerActivityProvider,
												ITimeZoneGuard timeZoneGuard)
		{
			_stateHolder = stateHolder;
			_skillGroupPerActivityProvider = skillGroupPerActivityProvider;
			_timeZoneGuard = timeZoneGuard;
		}

		public void Execute(DateOnlyPeriod period)
		{
			var cascadingSkills = _stateHolder().SchedulingResultState.CascadingSkills().ToArray();
			if (!cascadingSkills.Any())
				return;
			using (ResourceCalculationCurrent.PreserveContext())
			{
				using (new ResourceCalculationContextFactory(_stateHolder, () => new PersonSkillProvider(), _timeZoneGuard).Create(period))
				{
					foreach (var date in period.DayCollection())
					{
						var defaultResolution = cascadingSkills.First().DefaultResolution; //strange but cascading skills must have same resolution
						foreach (var activity in cascadingSkills.Select(x => x.Activity).Distinct())
						{
							foreach (var interval in date.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()).Intervals(TimeSpan.FromMinutes(defaultResolution)))
							{
								foreach (var skillGroup in _skillGroupPerActivityProvider.FetchOrdered(activity, interval))
								{
									//TODO - make this seperate classes/components if we want to keep this
									var resourcesMoved = shovelPerSkillGroupAndInterval(skillGroup, interval);
									reducePrimarySkillResources(skillGroup, interval, resourcesMoved);
								}
							}
						}
					}
				}
			}
		}

		private void reducePrimarySkillResources(CascadingSkillGroup skillGroup, DateTimePeriod interval, double resourcesMoved)
		{
			var stateHolder = _stateHolder();
			var primarySkillOverstaff = skillGroup.PrimarySkills
				.Sum(primarySkill => stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, 0).AbsoluteDifference);
			if (primarySkillOverstaff.IsZero())
				return;

			foreach (var primarySkill in skillGroup.PrimarySkills)
			{
				var skillStaffPeriod = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, 0);
				var overstaffForSkill = skillStaffPeriod.AbsoluteDifference;
				var percentageOverstaff = overstaffForSkill/primarySkillOverstaff;
				var resourceToSubtract = resourcesMoved*percentageOverstaff;
				skillStaffPeriod.SetCalculatedResource65(skillStaffPeriod.CalculatedResource - resourceToSubtract);
			}
		}

		private double shovelPerSkillGroupAndInterval(CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var resourcesMoved = 0d;
			var stateHolder = _stateHolder();
			var remainingResourcesInGroup = skillGroup.Resources;

			var remainingPrimarySkillOverstaff = skillGroup.PrimarySkills
				.Sum(primarySkill => stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(primarySkill, interval, int.MaxValue).AbsoluteDifference);

			if (!remainingPrimarySkillOverstaff.IsOverstaffed())
				return 0;

			foreach (var cascadingSkillGroupItem in skillGroup.CascadingSkillGroupItems)
			{
				var totalUnderstaffingInSkillGroup = cascadingSkillGroupItem.Skills
					.Select(skillToMoveTo => stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0).AbsoluteDifference)
					.Where(absoluteDifference => absoluteDifference.IsUnderstaffed())
					.Sum(absoluteDifference => -absoluteDifference);

				var remainingOverstaff = Math.Min(remainingPrimarySkillOverstaff, remainingResourcesInGroup);
				foreach (var skillToMoveTo in cascadingSkillGroupItem.Skills)
				{
					var skillStaffPeriodTo = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0);
					var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
					if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
						continue;

					var proportionalResourcesToMove = -skillToMoveToAbsoluteDifference / totalUnderstaffingInSkillGroup * remainingOverstaff;
					var resourceToMove = Math.Min(-skillToMoveToAbsoluteDifference, proportionalResourcesToMove);

					skillStaffPeriodTo.SetCalculatedResource65(skillStaffPeriodTo.CalculatedResource + resourceToMove);
					remainingResourcesInGroup -= resourceToMove;
					remainingPrimarySkillOverstaff -= resourceToMove;
					resourcesMoved += resourceToMove;
					if (remainingPrimarySkillOverstaff.IsZero() || remainingResourcesInGroup.IsZero())
						return resourcesMoved;
				}
			}
			return resourcesMoved;
		}	
	}
}