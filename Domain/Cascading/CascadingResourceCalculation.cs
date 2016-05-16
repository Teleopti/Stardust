﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class CascadingResourceCalculation
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public CascadingResourceCalculation(IResourceOptimizationHelper resourceOptimizationHelper, 
																Func<ISchedulerStateHolder> stateHolder)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_stateHolder = stateHolder;
		}

		public void ForDay(DateOnly date)
		{
			//TODO - we don't want to do this for every day, need a DateOnlyPeriod method as well?
			using (new ResourceCalculationContextFactory(_stateHolder, () => new CascadingPersonSkillProvider()).Create())
			{
				_resourceOptimizationHelper.ResourceCalculateDate(date, false, false); //check this later

				//just hack for now
				var stateHolder = _stateHolder();
				var skills = stateHolder.SchedulingResultState.Skills;
				var cascadingSkills = skills.Where(x => x.IsCascading()).OrderBy(x => x.CascadingIndex); //lägg nån annanstans
				foreach (var skillToMoveFrom in stateHolder.SchedulingResultState.Skills)
				{
					if (skillToMoveFrom.IsCascading())
					{
						var datetimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(date, date.AddDays(1), skillToMoveFrom.TimeZone);
						var intervals = datetimePeriod.Intervals(TimeSpan.FromMinutes(skillToMoveFrom.DefaultResolution));
						var skillStaffPeriodFromDic = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skillToMoveFrom];
						foreach (var interval in intervals)
						{
							ISkillStaffPeriod skillStaffPeriodFrom;
							if(skillStaffPeriodFromDic.TryGetValue(interval, out skillStaffPeriodFrom))
							{
								var skillToMoveFromAbsoluteDifference = skillStaffPeriodFrom.AbsoluteDifference;
								if (skillToMoveFromAbsoluteDifference > 0)
								{
									var calcStaffFrom = skillStaffPeriodFrom.CalculatedResource;
									var overstaffedValue = calcStaffFrom - skillToMoveFromAbsoluteDifference;

									//bara flytta till cascading skills som agent X kan?
									foreach (var skillToMoveTo in cascadingSkills)
									{
										if (!skillToMoveFrom.Activity.Equals(skillToMoveTo.Activity))
											continue;

										ISkillStaffPeriod skillStaffPeriodTo;
										var skillStaffPeriodToDic = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skillToMoveTo];
										if (skillStaffPeriodToDic.TryGetValue(interval, out skillStaffPeriodTo))
										{
											//TODO: bara flytta upp till 0
											if (skillStaffPeriodTo.AbsoluteDifference < 0)
											{
												skillStaffPeriodTo.SetCalculatedResource65(skillStaffPeriodTo.CalculatedResource + overstaffedValue);
												skillStaffPeriodFrom.SetCalculatedResource65(calcStaffFrom - overstaffedValue);
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
}