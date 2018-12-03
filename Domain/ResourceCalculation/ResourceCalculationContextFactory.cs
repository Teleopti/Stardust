using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationContextFactory
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly AddBpoResourcesToContext _addBpoResourcesToContext;

		public ResourceCalculationContextFactory(IPersonSkillProvider personSkillProvider, 
						ITimeZoneGuard timeZoneGuard, 
						AddBpoResourcesToContext addBpoResourcesToContext)
		{
			_personSkillProvider = personSkillProvider;
			_timeZoneGuard = timeZoneGuard;
			_addBpoResourcesToContext = addBpoResourcesToContext;
		}

		public IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, IEnumerable<ExternalStaff> bpoResources, bool primarySkillMode, DateOnlyPeriod period)
		{
			return new ResourceCalculationContext(createResources(scheduleDictionary, allSkills, bpoResources, primarySkillMode, period));
		}

		private Lazy<IResourceCalculationDataContainerWithSingleOperation> createResources(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, IEnumerable<ExternalStaff> bpoResources, bool primarySkillMode, DateOnlyPeriod period)
		{
			var dateTimePeriod = period.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone());
			var createResources = new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() =>
			{
				var minutesPerInterval = allSkills.Any() ?
					allSkills.Min(s => s.DefaultResolution)
					: 15;
				var extractor = new ScheduleProjectionExtractor(bpoResources, _personSkillProvider, minutesPerInterval, primarySkillMode);
				var ret = extractor.CreateRelevantProjectionList(scheduleDictionary, dateTimePeriod);
				if (scheduleDictionary.Scenario.DefaultScenario)
				{
					_addBpoResourcesToContext.Execute(ret, bpoResources);
				}
				return ret;
			});
			return createResources;
		}
	}
}