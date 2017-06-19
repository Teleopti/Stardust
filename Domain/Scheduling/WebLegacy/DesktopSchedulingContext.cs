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
			public desktopSchedulingContextData(ISchedulerStateHolder schedulerStateHolderFrom, ISchedulingOptionsProvider schedulingOptionsProvider)
			{
				SchedulerStateHolderFrom = schedulerStateHolderFrom;
				SchedulingOptionsProvider = schedulingOptionsProvider;
			}

			public ISchedulerStateHolder SchedulerStateHolderFrom { get; }
			public ISchedulingOptionsProvider SchedulingOptionsProvider { get; }
		}

		public IDisposable Set(ICommandIdentifier commandIdentifier, ISchedulerStateHolder schedulerStateHolderFrom, ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new desktopSchedulingContextData(schedulerStateHolderFrom, schedulingOptionsProvider));
		}


		public SchedulingOptions Fetch()
		{
			return ((desktopSchedulingContextData) _desktopContext.CurrentContext()).SchedulingOptionsProvider.Fetch();
		}
	}
}