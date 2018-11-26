using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	[RemoveMeWithToggle(Toggles.RTA_ReviewHistoricalAdherence_Domain_74770)]
	public class AdherencePercentageCalculator
	{
		public int? Calculate(DateTimePeriod? shift, IEnumerable<DateTimePeriod> neutralPeriods, IEnumerable<DateTimePeriod> outPeriods, DateTime now)
		{
			if (!shift.HasValue)
				return null;

			var calculateUntil = new[] {now, shift.Value.EndDateTime}.Min();

			var shiftTime = calculateUntil - shift.Value.StartDateTime;

			var timeNeutral = time(neutralPeriods);
			var timeOut = time(outPeriods);
			var timeIn = shiftTime - timeOut - timeNeutral;

			var timeToAdhere = shiftTime - timeNeutral;

			if (timeToAdhere == TimeSpan.Zero)
				return null;
			return Convert.ToInt32((timeIn.TotalSeconds / timeToAdhere.TotalSeconds) * 100);
		}

		private static TimeSpan time(IEnumerable<DateTimePeriod> periods) =>
			TimeSpan.FromSeconds(periods.Sum(x => (x.EndDateTime - x.StartDateTime).TotalSeconds));
	}
}