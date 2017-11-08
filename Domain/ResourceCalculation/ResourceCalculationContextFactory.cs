using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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

		public IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, IEnumerable<BpoResource> bpoResources, bool primarySkillMode, DateOnlyPeriod period)
		{
			return new ResourceCalculationContext(createResources(scheduleDictionary, allSkills, bpoResources, primarySkillMode, period));
		}

		private Lazy<IResourceCalculationDataContainerWithSingleOperation> createResources(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, IEnumerable<BpoResource> bpoResources, bool primarySkillMode, DateOnlyPeriod period)
		{
			var createResources = new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() =>
			{
				var minutesPerInterval = allSkills.Any() ?
					allSkills.Min(s => s.DefaultResolution)
					: 15;
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval, primarySkillMode);
				var ret = extractor.CreateRelevantProjectionList(scheduleDictionary, period.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone()));
				tempFix(ret, bpoResources);
				return ret;
			});
			return createResources;
		}

		private void tempFix(ResourceCalculationDataContainer ret, IEnumerable<BpoResource> bpoResources)
		{
			if (bpoResources == null)
				return;

			foreach (var bpoResource in bpoResources)
			{
				var tempAgent = new Person();
				var period = new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
				period.AddPersonSkill(new PersonSkill(bpoResource.Skills.Single(), new Percent(1)));
				tempAgent.AddPersonPeriod(period);
				var resLayer = new ResourceLayer
				{
					PayloadId = bpoResource.Skills.Single().Activity.Id.Value,
					Period = bpoResource.Period,
					Resource = bpoResource.Resources
				};
				ret.AddResources(tempAgent, DateOnly.Today, resLayer);
			}
		}
	}
}