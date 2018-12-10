using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class MaxSeatPeakData
	{
		private readonly IDictionary<DateOnly, double> _datePeaks;

		public MaxSeatPeakData(IDictionary<DateOnly, double> datePeaks)
		{
			_datePeaks = datePeaks;
		}

		public bool HasPeaks()
		{
			return _datePeaks.Values.Any(value => value.IsPositive());
		}

		public bool IsBetterThan(MaxSeatPeakData maxPeaksBefore, IEnumerable<DateOnly> modifiedDates)
		{
			var highestPeakBefore = 0d;
			var highestPeakCurrent = 0d;
			foreach (var modifiedDate in modifiedDates)
			{
				highestPeakBefore = Math.Max(highestPeakBefore, maxPeaksBefore._datePeaks[modifiedDate]);
				highestPeakCurrent = Math.Max(highestPeakCurrent, _datePeaks[modifiedDate]);
			}

			return highestPeakCurrent < highestPeakBefore;
		}
	}
}