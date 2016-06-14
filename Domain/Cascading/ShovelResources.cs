using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
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
									shovelPerSkillGroupAndInterval(skillGroup, interval);
								}
							}
						}
					}
				}
			}
		}

		private void shovelPerSkillGroupAndInterval(CascadingSkillGroup skillGroup, DateTimePeriod interval)
		{
			var stateHolder = _stateHolder();
			var remainingResourcesInGroup = skillGroup.Resources;
			var skillStaffPeriodFrom = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillGroup.PrimarySkill, interval, int.MaxValue);
			var remainingOverstaff = skillStaffPeriodFrom.AbsoluteDifference;
			if (!remainingOverstaff.IsOverstaffed())
				return;
			foreach (var cascadingSkillGroupItem in skillGroup.CascadingSkillGroupItems)
			{
				var resourcesToMove = Math.Min(remainingOverstaff, remainingResourcesInGroup)/cascadingSkillGroupItem.NumberOfSkills;
				foreach (var skillToMoveTo in cascadingSkillGroupItem.Skills)
				{
					var skillStaffPeriodTo = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0);
					var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
					if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
						continue;
					resourcesToMove = Math.Min(-skillToMoveToAbsoluteDifference, resourcesToMove);
					skillStaffPeriodTo.TakeResourcesFrom(skillStaffPeriodFrom, resourcesToMove);
					remainingResourcesInGroup -= resourcesToMove;
					remainingOverstaff -= resourcesToMove;
					if (remainingOverstaff.IsZero() || remainingResourcesInGroup.IsZero())
						return;
				}
			}
		}
	}
}