using System;
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
				foreach (var skill in stateHolder.SchedulingResultState.Skills)
				{
					if (skill.IsCascading())
					{
						var datetimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(date, date.AddDays(1), skill.TimeZone);
						var intervals = datetimePeriod.Intervals(TimeSpan.FromMinutes(skill.DefaultResolution));
						var skillStaffPeriodDic = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[skill];
						foreach (var interval in intervals)
						{
							ISkillStaffPeriod skillStaffPeriodFrom;
							if(skillStaffPeriodDic.TryGetValue(interval, out skillStaffPeriodFrom))
							{
								if (skillStaffPeriodFrom.AbsoluteDifference > 0)
								{
									var calcStaffFrom = skillStaffPeriodFrom.CalculatedResource;
									var overstaffedValue = calcStaffFrom - skillStaffPeriodFrom.AbsoluteDifference;
									//temp
									var lastCascadingSkill = cascadingSkills.Last();
									var skillStaffPeriodTo = stateHolder.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary[lastCascadingSkill][interval]; //kolla om öppet
									skillStaffPeriodTo.SetCalculatedResource65(skillStaffPeriodTo.CalculatedResource + overstaffedValue);
									skillStaffPeriodFrom.SetCalculatedResource65(skillStaffPeriodFrom.CalculatedResource - overstaffedValue);
								}
							}
						}
					}
				}
			}
		}
	}
}