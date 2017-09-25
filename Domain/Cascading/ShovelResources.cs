using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ShovelResources
	{
		private readonly ReducePrimarySkillResources _reducePrimarySkillResources;
		private readonly AddResourcesToSubSkills _addResourcesToSubSkills;
		private readonly SkillSetPerActivityProvider _skillSetPerActivityProvider;
		private readonly PrimarySkillOverstaff _primarySkillOverstaff;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ShovelResources(ReducePrimarySkillResources reducePrimarySkillResources,
			AddResourcesToSubSkills addResourcesToSubSkills,
			SkillSetPerActivityProvider skillSetPerActivityProvider,
			PrimarySkillOverstaff primarySkillOverstaff,
			ITimeZoneGuard timeZoneGuard)
		{
			_reducePrimarySkillResources = reducePrimarySkillResources;
			_addResourcesToSubSkills = addResourcesToSubSkills;
			_skillSetPerActivityProvider = skillSetPerActivityProvider;
			_primarySkillOverstaff = primarySkillOverstaff;
			_timeZoneGuard = timeZoneGuard;
		}

		public void Execute(IShovelResourceData shovelResourceData, IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod period, IShovelingCallback shovelingCallback, Func<IDisposable> getResourceCalculationContext)
		{
			var cascadingSkills = new CascadingSkills(allSkills); 
			if (!cascadingSkills.Any())
				return;

			var activitiesAndIntervalLengths = cascadingSkills.AffectedActivities().ToArray();
			shovelingCallback.BeforeShoveling(shovelResourceData);
			using (ResourceCalculationCurrent.PreserveContext())
			{
				var context = getResourceCalculationContext == null ? getDefaultContext(scheduleDictionary, allSkills, period) : getResourceCalculationContext();
					
				using (context)
				{
					foreach (var date in period.DayCollection())
					{
						foreach (var activityAndIntervalLength in activitiesAndIntervalLengths)
						{
							foreach (var interval in date.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()).Intervals(activityAndIntervalLength.IntervalLength))
							{
								var orderedSkillGroups = _skillSetPerActivityProvider.FetchOrdered(cascadingSkills, ResourceCalculationContext.Fetch(), activityAndIntervalLength.Activity, interval);
								var allSkillGroups = orderedSkillGroups.AllSkillSets();
								foreach (var skillGroupsWithSameIndex in orderedSkillGroups)
								{
									var state = _primarySkillOverstaff.AvailableSum(shovelResourceData, allSkillGroups, skillGroupsWithSameIndex, interval);
									_addResourcesToSubSkills.Execute(state, shovelResourceData, skillGroupsWithSameIndex, interval, shovelingCallback);
									_reducePrimarySkillResources.Execute(state, shovelResourceData, interval, skillGroupsWithSameIndex, shovelingCallback);
								}
							}
						}
					}
				}
			}
		}

		private IDisposable getDefaultContext(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod period)
		{
			var rcf = new ResourceCalculationContextFactory(new PersonSkillProvider(), _timeZoneGuard);
			return rcf.Create(scheduleDictionary, allSkills, false, period);
		}

	}
}