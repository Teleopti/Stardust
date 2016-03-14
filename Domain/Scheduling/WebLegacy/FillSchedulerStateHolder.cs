using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class FillSchedulerStateHolder : IFillSchedulerStateHolder
	{
		private ISchedulerStateHolder _schedulerStateHolderFrom;

		public WebSchedulingSetupResult Fill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			foreach (var person in _schedulerStateHolderFrom.AllPermittedPersons)
			{
				schedulerStateHolderTo.AllPermittedPersons.Add(person);
			}

			schedulerStateHolderTo.SchedulingResultState.Schedules = _schedulerStateHolderFrom.SchedulingResultState.Schedules;
			return null;
		}

		public IDisposable Add(ISchedulerStateHolder schedulerStateHolderFrom)
		{
			_schedulerStateHolderFrom = schedulerStateHolderFrom;
			return new GenericDisposable(() => _schedulerStateHolderFrom = null);
		}
	}
}