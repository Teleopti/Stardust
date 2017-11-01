using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopSchedulingContext : ISchedulingOptionsProvider, ICurrentSchedulingCallback
	{
		private readonly DesktopContext _desktopContext;

		public DesktopSchedulingContext(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		private class desktopSchedulingContextData : IDesktopContextData
		{
			public desktopSchedulingContextData(ISchedulerStateHolder schedulerStateHolderFrom, SchedulingOptions schedulingOptions, ISchedulingCallback schedulingCallback)
			{
				SchedulerStateHolderFrom = schedulerStateHolderFrom;
				SchedulingOptions = schedulingOptions;
				SchedulingCallback = schedulingCallback;
			}

			public ISchedulerStateHolder SchedulerStateHolderFrom { get; }
			public SchedulingOptions SchedulingOptions { get; }
			public ISchedulingCallback SchedulingCallback { get; }
		}

		public IDisposable Set(ICommandIdentifier commandIdentifier, ISchedulerStateHolder schedulerStateHolderFrom, SchedulingOptions schedulingOptions, ISchedulingCallback schedulingCallback)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new desktopSchedulingContextData(schedulerStateHolderFrom, schedulingOptions, schedulingCallback));
		}


		public SchedulingOptions Fetch(IDayOffTemplate defaultDayOffTemplate)
		{
			return ((desktopSchedulingContextData) _desktopContext.CurrentContext()).SchedulingOptions.Clone();
		}

		public ISchedulingCallback Current()
		{
			return ((desktopSchedulingContextData)_desktopContext.CurrentContext()).SchedulingCallback;
		}
	}
}