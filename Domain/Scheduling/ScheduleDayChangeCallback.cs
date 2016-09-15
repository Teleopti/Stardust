using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ScheduleDayChangeCallback : IScheduleDayChangeCallback
	{
		private readonly ISharedResourceContext _sharedResourceContext;
		private readonly IResourceCalculateDaysDecider _resourceCalculateDaysDecider;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public ScheduleDayChangeCallback(ISharedResourceContext sharedResourceContext, 
																		IResourceCalculateDaysDecider resourceCalculateDaysDecider, 
																		Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_sharedResourceContext = sharedResourceContext;
			_resourceCalculateDaysDecider = resourceCalculateDaysDecider;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void ScheduleDayBeforeChanging()
		{
			_sharedResourceContext.MakeSureExists(new DateOnlyPeriod(), false);
		}

		public void ScheduleDayChanged(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			applyChangesToResourceContainer(partBefore, partAfter);
			markDaysToRecalculate(partBefore, partAfter);
		}

		private static void applyChangesToResourceContainer(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			var container = ResourceCalculationContext.Fetch();
			container.RemoveScheduleDayFromContainer(partBefore, container.MinSkillResolution);
			container.AddScheduleDayToContainer(partAfter, container.MinSkillResolution);
		}

		private void markDaysToRecalculate(IScheduleDay partBefore, IScheduleDay partAfter)
		{
			_resourceCalculateDaysDecider.DecideDates(partAfter, partBefore).ForEach(_schedulerStateHolder().MarkDateToBeRecalculated);
		}
	}
}