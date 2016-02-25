using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class NormalResourceCalculationContext
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;

		public NormalResourceCalculationContext(Func<ISchedulerStateHolder> schedulerStateHolder, Func<IPersonSkillProvider> personSkillProvider)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_personSkillProvider = personSkillProvider;
		}

		public IDisposable Create()
		{
			var minutesPerInterval = 15;

			if (_schedulerStateHolder().SchedulingResultState.Skills.Any())
			{
				minutesPerInterval = _schedulerStateHolder().SchedulingResultState.Skills.Min(s => s.DefaultResolution);
			}
			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(_schedulerStateHolder().Schedules);
			return new ResourceCalculationContext(resources);
		} 
	}
}