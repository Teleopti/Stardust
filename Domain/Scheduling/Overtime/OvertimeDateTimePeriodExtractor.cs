using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IOvertimeDateTimePeriodExtractor
	{
		IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDurantion, DateTimePeriod shiftPeriod, DateTimePeriod specificPeriod);
	}

	public class OvertimeDateTimePeriodExtractor : IOvertimeDateTimePeriodExtractor
	{
		public IList<IOvertimeDateTimePeriodHolder> Extract(int minimumResolution, MinMax<TimeSpan> overtimeDurantion, DateTimePeriod shiftPeriod, DateTimePeriod specificPeriod)
		{
			IList<IOvertimeDateTimePeriodHolder>  dateTimePeriodHolders = new List<IOvertimeDateTimePeriodHolder>();
			var shiftStart = shiftPeriod.StartDateTime;
			var shiftEnd = shiftPeriod.EndDateTime;

			var intersection = specificPeriod.Intersection(shiftPeriod);
			if (intersection == null && !specificPeriod.AdjacentTo(shiftPeriod)) return dateTimePeriodHolders;

			for (var minutes = minimumResolution; minutes <= overtimeDurantion.Maximum.TotalMinutes; minutes += minimumResolution)
			{
				var duration = TimeSpan.FromMinutes(minutes);
				if (duration < overtimeDurantion.Minimum) continue;

				var periodBefore = new DateTimePeriod(shiftStart.Add(-duration), shiftStart);

				if (specificPeriod.Contains(periodBefore))
				{
					var dateTimePeriodHolderBefore = new OvertimeDateTimePeriodHolder();
					dateTimePeriodHolderBefore.Add(periodBefore);
					dateTimePeriodHolders.Add(dateTimePeriodHolderBefore);
				}

				var periodAfter = new DateTimePeriod(shiftEnd, shiftEnd.Add(duration));
				if (specificPeriod.Contains(periodAfter))
				{
					var dateTimePeriodHolderAfter = new OvertimeDateTimePeriodHolder();
					dateTimePeriodHolderAfter.Add(periodAfter);
					dateTimePeriodHolders.Add(dateTimePeriodHolderAfter);
				}	
			}

			return dateTimePeriodHolders;
		}
	}
}
