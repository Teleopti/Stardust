using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationContextFactory
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ResourceCalculationContextFactory(Func<ISchedulerStateHolder> schedulerStateHolder, Func<IPersonSkillProvider> personSkillProvider, ITimeZoneGuard timeZoneGuard)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_personSkillProvider = personSkillProvider;
			_timeZoneGuard = timeZoneGuard;
		}

		public IDisposable Create()
		{
			return new ResourceCalculationContext(createResources(null));
		}

		public IDisposable Create(DateOnlyPeriod period)
		{
			return new ResourceCalculationContext(createResources(period));
		}

		private Lazy<IResourceCalculationDataContainerWithSingleOperation> createResources(DateOnlyPeriod? period)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var createResources = new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() =>
			{
				var minutesPerInterval = 15;

				if (schedulerStateHolder.SchedulingResultState.Skills.Any())
				{
					minutesPerInterval = schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
				}
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
				return period.HasValue ? 
					extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules, period.Value.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone())) : 
					extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules);
			});
			return createResources;
		}
	}
}