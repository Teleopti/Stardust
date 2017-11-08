using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationContextFactory
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ResourceCalculationContextFactory(IPersonSkillProvider personSkillProvider, ITimeZoneGuard timeZoneGuard)
		{
			_personSkillProvider = personSkillProvider;
			_timeZoneGuard = timeZoneGuard;
		}

		public IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, bool primarySkillMode, DateOnlyPeriod period)
		{
			return new ResourceCalculationContext(createResources(scheduleDictionary, allSkills, primarySkillMode, period));
		}

		private Lazy<IResourceCalculationDataContainerWithSingleOperation> createResources(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, bool primarySkillMode, DateOnlyPeriod period)
		{
			var createResources = new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() =>
			{
				var minutesPerInterval = allSkills.Any() ?
					allSkills.Min(s => s.DefaultResolution)
					: 15;
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval, primarySkillMode);
				return extractor.CreateRelevantProjectionList(scheduleDictionary, period.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()));
			});
			return createResources;
		}
	}
}