using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class AdherencePercentageCalculator
	{
		public int? Calculate(
			DateTime? shiftStartTime,
			DateTime? shiftEndTime,
			IEnumerable<HistoricalAdherence> data,
			DateTime now)
		{
			if (!shiftStartTime.HasValue)
				return null;

			var onGoingShift = now < shiftEndTime.Value;
			var calculateUntil = onGoingShift ? now : shiftEndTime.Value;
			var adherenceAtStart = data.LastOrDefault(x => x.Timestamp <= shiftStartTime)?.Adherence ?? HistoricalAdherenceAdherence.Neutral;

			var adherenceReadModels = data
				.Select(a => new adherenceMoment {Time = a.Timestamp, Adherence = a.Adherence})
				.Append(new adherenceMoment {Time = shiftStartTime.Value, Adherence = adherenceAtStart})
				.Append(new adherenceMoment {Time = calculateUntil})
				.Where(a =>
				{
					var isOnShift = a.Time >= shiftStartTime.Value && a.Time <= shiftEndTime.Value;
					return isOnShift;
				})
				.OrderBy(x => x.Time)
				.ToArray();

			var timeInAdherence = timeIn(HistoricalAdherenceAdherence.In, adherenceReadModels);
			var timeInNeutral = timeIn(HistoricalAdherenceAdherence.Neutral, adherenceReadModels);
			var shiftTime = shiftEndTime.Value - shiftStartTime.Value;
			var timeToAdhere = shiftTime - timeInNeutral;

			if (timeToAdhere == TimeSpan.Zero)
				return 0;
			return Convert.ToInt32((timeInAdherence.TotalSeconds / timeToAdhere.TotalSeconds) * 100);
		}

		private static TimeSpan timeIn(
			HistoricalAdherenceAdherence adherenceType,
			IEnumerable<adherenceMoment> data) =>
			data.Aggregate(new timeAccumulated(), (t, m) =>
			{
				if (t.AccumulateSince != null)
					t.AccumulatedTime += (m.Time - t.AccumulateSince).Value;
				t.AccumulateSince = m.Adherence == adherenceType ? m?.Time : null;
				return t;
			}).AccumulatedTime;

		private class timeAccumulated
		{
			public TimeSpan AccumulatedTime { get; set; }
			public DateTime? AccumulateSince { get; set; }
		}

		private class adherenceMoment
		{
			public HistoricalAdherenceAdherence? Adherence { get; set; }
			public DateTime Time { get; set; }
		}
	}
}