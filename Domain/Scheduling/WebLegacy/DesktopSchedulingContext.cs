using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	//2 be continued - not in use at the moment....
	public class DesktopSchedulingContext : ISchedulingOptionsProvider
	{
		private readonly DesktopContext _desktopContext;

		public DesktopSchedulingContext(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		private class desktopSchedulingContextData : IDesktopContextData
		{
			public desktopSchedulingContextData(ISchedulerStateHolder schedulerStateHolderFrom, SchedulingOptions schedulingOptions)
			{
				SchedulerStateHolderFrom = schedulerStateHolderFrom;
				SchedulingOptions = schedulingOptions;
			}

			public ISchedulerStateHolder SchedulerStateHolderFrom { get; }
			public SchedulingOptions SchedulingOptions { get; }
		}

		public IDisposable Set(ICommandIdentifier commandIdentifier, ISchedulerStateHolder schedulerStateHolderFrom, SchedulingOptions schedulingOptions)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new desktopSchedulingContextData(schedulerStateHolderFrom, schedulingOptions));
		}


		public SchedulingOptions Fetch()
		{
			return ((desktopSchedulingContextData) _desktopContext.CurrentContext()).SchedulingOptions;
		}
	}
}