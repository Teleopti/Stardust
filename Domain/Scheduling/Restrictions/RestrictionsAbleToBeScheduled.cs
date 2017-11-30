using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class RestrictionsAbleToBeScheduled
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		
		public RestrictionsAbleToBeScheduled(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public bool Execute(IVirtualSchedulePeriod schedulePeriod)
		{
			return !_schedulerStateHolder().Schedules.Any();
		}
	}
}