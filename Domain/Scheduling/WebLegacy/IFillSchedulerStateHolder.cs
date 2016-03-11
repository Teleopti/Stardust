using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public interface IFillSchedulerStateHolder
	{
		WebSchedulingSetupResult Fill(ISchedulerStateHolder schedulerStateHolder, DateOnlyPeriod period);
	}

	public class FillSchedulerStateHolder : IFillSchedulerStateHolder
	{
		private ISchedulerStateHolder _schedulerStateHolderFrom;

		public FillSchedulerStateHolder()
		{
			FilledSchedulerStateHolder = null;
		}

		public ISchedulerStateHolder FilledSchedulerStateHolder
		{
			get { return _schedulerStateHolderFrom; }
			private set { _schedulerStateHolderFrom = value; }
		}

		public WebSchedulingSetupResult Fill(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period)
		{
			foreach (var person in _schedulerStateHolderFrom.AllPermittedPersons)
			{
				schedulerStateHolderTo.AllPermittedPersons.Add(person);
			}

			schedulerStateHolderTo.SchedulingResultState.Schedules = _schedulerStateHolderFrom.SchedulingResultState.Schedules;
			FilledSchedulerStateHolder = schedulerStateHolderTo;

			return null;
		}

		public IDisposable Add(ISchedulerStateHolder schedulerStateHolderFrom)
		{
			_schedulerStateHolderFrom = schedulerStateHolderFrom;
			return new GenericDisposable(() => _schedulerStateHolderFrom = null);
		}
	}
}