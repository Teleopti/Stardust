using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle("Remove impl of IResourceCalculationContextFactory on this class", Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class ResourceCalculationContextFactory : IResourceCalculationContextFactory
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ResourceCalculationContextFactory(IPersonSkillProvider personSkillProvider, ITimeZoneGuard timeZoneGuard)
		{
			_personSkillProvider = personSkillProvider;
			_timeZoneGuard = timeZoneGuard;
		}

		public IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills)
		{
			return new ResourceCalculationContext(createResources(scheduleDictionary, allSkills, null));
		}

		public IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod period)
		{
			return new ResourceCalculationContext(createResources(scheduleDictionary, allSkills, period));
		}

		private Lazy<IResourceCalculationDataContainerWithSingleOperation> createResources(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, DateOnlyPeriod? period)
		{
			var createResources = new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() =>
			{
				var minutesPerInterval = allSkills.Any() ?
					allSkills.Min(s => s.DefaultResolution) 
					: 15;
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval);
				return period.HasValue ? 
					extractor.CreateRelevantProjectionList(scheduleDictionary, period.Value.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone())) : 
					extractor.CreateRelevantProjectionList(scheduleDictionary);
			});
			return createResources;
		}
	}
}