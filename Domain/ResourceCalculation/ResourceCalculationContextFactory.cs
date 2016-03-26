﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationContextFactory
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;

		public ResourceCalculationContextFactory(Func<ISchedulerStateHolder> schedulerStateHolder, Func<IPersonSkillProvider> personSkillProvider)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_personSkillProvider = personSkillProvider;
		}

		public IDisposable Create()
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var minutesPerInterval = 15;

			if (schedulerStateHolder.SchedulingResultState.Skills.Any())
			{
				minutesPerInterval = schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
			}
			var extractor = new ScheduleProjectionExtractor(_personSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules);
			return new ResourceCalculationContext(resources);
		}

		public IDisposable CreateForShoveling(Func<IPersonSkillProvider> personSkillProvider)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var minutesPerInterval = 15;

			if (schedulerStateHolder.SchedulingResultState.Skills.Any())
			{
				minutesPerInterval = schedulerStateHolder.SchedulingResultState.Skills.Min(s => s.DefaultResolution);
			}
			var extractor = new ScheduleProjectionExtractor(personSkillProvider(), minutesPerInterval);
			var resources = extractor.CreateRelevantProjectionList(schedulerStateHolder.Schedules);
			return new ResourceCalculationContext(resources);
		} 
	}
}