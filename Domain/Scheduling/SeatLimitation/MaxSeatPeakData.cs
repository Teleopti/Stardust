using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

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
			foreach (var date in modifiedDates)
			{
				var currentPeak = _datePeaks[date];
				var prevPeak = maxPeaksBefore._datePeaks[date];
				if (currentPeak.IsPositive() && currentPeak >= prevPeak)
					return false;
			}
			return true;
		}
	}
}