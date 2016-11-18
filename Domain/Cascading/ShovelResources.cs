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
		private readonly AddResourcesToSubSkills _addResourcesToSubSkills;
		private readonly ReducePrimarySkillResources _reducePrimarySkillResources;
		private readonly SkillGroupPerActivityProvider _skillGroupPerActivityProvider;
		private readonly PrimarySkillOverstaff _primarySkillOverstaff;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ShovelResources(AddResourcesToSubSkills addResourcesToSubSkills,
			ReducePrimarySkillResources reducePrimarySkillResources,
			SkillGroupPerActivityProvider skillGroupPerActivityProvider,
			PrimarySkillOverstaff primarySkillOverstaff,
			ITimeZoneGuard timeZoneGuard)
		{
			_addResourcesToSubSkills = addResourcesToSubSkills;
			_reducePrimarySkillResources = reducePrimarySkillResources;
			_skillGroupPerActivityProvider = skillGroupPerActivityProvider;
			_primarySkillOverstaff = primarySkillOverstaff;
			_timeZoneGuard = timeZoneGuard;
		}

		public void Execute(ISkillStaffPeriodHolder skillStaffPeriodHolder, IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod period)
		{
			var cascadingSkills = new CascadingSkills(allSkills); 
			if (!cascadingSkills.Any())
				return;

			var defaultResolution = TimeSpan.FromMinutes(cascadingSkills.First().DefaultResolution); //strange but cascading skills must have same resolution.
			var activities = cascadingSkills.AffectedActivities().ToArray();

			using (ResourceCalculationCurrent.PreserveContext())
			{
				using (new ResourceCalculationContextFactory(new PersonSkillProvider(), _timeZoneGuard).Create(scheduleDictionary, allSkills, false, period))
				{
					foreach (var date in period.DayCollection())
					{
						foreach (var activity in activities)
						{
							foreach (var interval in date.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()).Intervals(defaultResolution))
							{
								var orderedSkillGroups = _skillGroupPerActivityProvider.FetchOrdered(cascadingSkills, activity, interval);
								var allSkillGroups = orderedSkillGroups.AllSkillGroups();
								foreach (var skillGroupsWithSameIndex in orderedSkillGroups)
								{
									var state = _primarySkillOverstaff.AvailableSum(skillStaffPeriodHolder, allSkillGroups, skillGroupsWithSameIndex, interval);
									_addResourcesToSubSkills.Execute(state, skillStaffPeriodHolder, skillGroupsWithSameIndex, interval);
									_reducePrimarySkillResources.Execute(state, skillStaffPeriodHolder, interval);
								}
							}
						}
					}
				}
			}
		}
	}
}