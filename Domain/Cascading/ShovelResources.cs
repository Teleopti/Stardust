using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
			var stateHolder = _stateHolder();
			var cascadingSkills = stateHolder.SchedulingResultState.CascadingSkills().ToArray();
			if (!cascadingSkills.Any())
				return;

			using (ResourceCalculationCurrent.PreserveContext())
			{
				using (new ResourceCalculationContextFactory(_stateHolder, () => new PersonSkillProvider(), _timeZoneGuard).Create(period))
				{
					foreach (var date in period.DayCollection())
					{
						var defaultResolution = cascadingSkills.First().DefaultResolution; //(maybe) correct...
						foreach (var activity in cascadingSkills.Select(x => x.Activity).Distinct())
						{
							foreach (var interval in date.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()).Intervals(TimeSpan.FromMinutes(defaultResolution)))
							{
								foreach (var skillGroup in _skillGroupPerActivityProvider.FetchOrdered(activity, interval))
								{
									var remainingResourcesInGroup = skillGroup.Resources;
									var skillStaffPeriodFrom = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillGroup.PrimarySkill, interval, int.MaxValue);
									var remainingOverstaff = skillStaffPeriodFrom.AbsoluteDifference;
									if (!remainingOverstaff.IsOverstaffed())
										continue;
									foreach (var skillsToMoveTo in skillGroup.CascadingSkills)
									{
										var numOfSkills = skillsToMoveTo.NumberOfSkills;
										var resourcesToMove = Math.Min(remainingOverstaff, remainingResourcesInGroup);
										resourcesToMove = resourcesToMove / numOfSkills;

										foreach (var skillToMoveTo in skillsToMoveTo.Skills)
										{
											var skillStaffPeriodTo = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillStaffPeriodOrDefault(skillToMoveTo, interval, 0);
											var skillToMoveToAbsoluteDifference = skillStaffPeriodTo.AbsoluteDifference;
											if (!skillToMoveToAbsoluteDifference.IsUnderstaffed())
												continue;
											resourcesToMove = Math.Min(-skillToMoveToAbsoluteDifference, resourcesToMove);
											
											skillStaffPeriodTo.TakeResourcesFrom(skillStaffPeriodFrom, resourcesToMove);
											remainingResourcesInGroup -= resourcesToMove;
											remainingOverstaff -= resourcesToMove;
											//TODO: jump out outer loop as well?
											if (remainingOverstaff.IsZero() || remainingResourcesInGroup.IsZero())
												break;
										}		
									}
								}
							}
						}
					}
				}
			}
		}
	}
}