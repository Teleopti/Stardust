using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeSchedulingService
	{
		void Execute(IList<IPerson> persons, DateOnly dateOnly, IActivity activity, MinMax<TimeSpan> duration);
	}
	public class OvertimeSchedulingService : IOvertimeSchedulingService
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IOvertimeLengthDecider _overtimeLengthDecider;

		public OvertimeSchedulingService(ISchedulingResultStateHolder schedulingResultStateHolder,
		                                 IOvertimeLengthDecider overtimeLengthDecider)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_overtimeLengthDecider = overtimeLengthDecider;
		}

		public void Execute(IList<IPerson> persons, DateOnly dateOnly, IActivity activity, MinMax<TimeSpan> duration)
		{
			var person = selectPersonRandomly(persons, dateOnly);
			var schedule = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
			var overtimeLayerLength = _overtimeLengthDecider.Decide(person, dateOnly, schedule.Period.EndDateTime, activity, duration);


		}

		private IPerson selectPersonRandomly(IList<IPerson> persons, DateOnly dateOnly)
		{
			var personsHaveNoOvertime = new List<IPerson>();
			foreach (var person in persons)
			{
				var schedule = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
				if (schedule.PersonAssignment().ProjectionService().CreateProjection().Overtime() > TimeSpan.Zero)
					continue;
				personsHaveNoOvertime.Add(person);
			}
			if (personsHaveNoOvertime.Count > 0)
				return personsHaveNoOvertime.GetRandom();
			return null;
		}
	}
}