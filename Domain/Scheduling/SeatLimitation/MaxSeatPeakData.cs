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

		public bool IsNotBetterThan(IEnumerable<DateOnly> dates, MaxSeatPeakData maxPeaksBefore)
		{
			return _datePeaks
				.Any(keyValueAfter => keyValueAfter.Value.IsPositive() && 
						keyValueAfter.Value >= maxPeaksBefore._datePeaks[keyValueAfter.Key] && 
						dates.Contains(keyValueAfter.Key));
		}
	}
}