using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResources
	{
		private readonly IShovelResourcesPerActivityIntervalSkillGroup _shovelResourcesPerActivityIntervalSkillGroup;
		private readonly SkillGroupPerActivityProvider _skillGroupPerActivityProvider;
		private readonly PrimarySkillOverstaff _primarySkillOverstaff;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ShovelResources(IShovelResourcesPerActivityIntervalSkillGroup shovelResourcesPerActivityIntervalSkillGroup,
			SkillGroupPerActivityProvider skillGroupPerActivityProvider,
			PrimarySkillOverstaff primarySkillOverstaff,
			ITimeZoneGuard timeZoneGuard)
		{
			_shovelResourcesPerActivityIntervalSkillGroup = shovelResourcesPerActivityIntervalSkillGroup;
			_skillGroupPerActivityProvider = skillGroupPerActivityProvider;
			_primarySkillOverstaff = primarySkillOverstaff;
			_timeZoneGuard = timeZoneGuard;
		}

		public void Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod period)
		{
			var cascadingSkills = new CascadingSkills(allSkills); 
			if (!cascadingSkills.Any())
				return;

			var defaultResolution = cascadingSkills.First().DefaultResolution; //strange but cascading skills must have same resolution.
			var activities = cascadingSkills.AffectedActivities().ToArray();

			using (ResourceCalculationCurrent.PreserveContext())
			{
				using (new ResourceCalculationContextFactory(() => new PersonSkillProvider(), _timeZoneGuard).Create(scheduleDictionary, allSkills, period))
				{
					foreach (var date in period.DayCollection())
					{
						foreach (var activity in activities)
						{
							foreach (var interval in date.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()).Intervals(TimeSpan.FromMinutes(defaultResolution)))
							{
								foreach (var skillGroup in _skillGroupPerActivityProvider.FetchOrdered(cascadingSkills, activity, interval))
								{
									var primarySkillOverstaff = _primarySkillOverstaff.Sum(skillStaffPeriodHolder, skillGroup, interval);
									var shovelResourcesState = new ShovelResourcesState(skillGroup.Resources, primarySkillOverstaff);
									_shovelResourcesPerActivityIntervalSkillGroup.Execute(shovelResourcesState, skillStaffPeriodHolder, activity, interval, skillGroup);
								}
							}
						}
					}
				}
			}
		}
	}
}