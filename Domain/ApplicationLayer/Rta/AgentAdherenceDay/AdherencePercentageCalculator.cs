using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay
{
	public class AdherencePercentageCalculator : IAdherencePercentageCalculator
	{
		private readonly INow _now;

		public AdherencePercentageCalculator(INow now)
		{
			_now = now;
		}

		public int? CalculatePercentage(DateTime? shiftStartTime, DateTime? shiftEndTime,
			IEnumerable<HistoricalAdherenceReadModel> data)
		{
			if (!shiftStartTime.HasValue)
				return null;

			var onGoingShift = _now.UtcDateTime() < shiftEndTime.Value;
			var calculateUntil = onGoingShift ? _now.UtcDateTime() : shiftEndTime.Value;
			var adherenceAtStart = data.LastOrDefault(x => x.Timestamp <= shiftStartTime)?.Adherence ?? HistoricalAdherenceReadModelAdherence.Neutral;

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

			var timeInAdherence = timeIn(HistoricalAdherenceReadModelAdherence.In, adherenceReadModels);
			var timeInNeutral = timeIn(HistoricalAdherenceReadModelAdherence.Neutral, adherenceReadModels);
			var shiftTime = shiftEndTime.Value - shiftStartTime.Value;
			var timeToAdhere = shiftTime - timeInNeutral;

			if (timeToAdhere == TimeSpan.Zero)
				return 0;
			return Convert.ToInt32((timeInAdherence.TotalSeconds / timeToAdhere.TotalSeconds) * 100);
		}

		private static TimeSpan timeIn(HistoricalAdherenceReadModelAdherence adherenceType,
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
			public HistoricalAdherenceReadModelAdherence? Adherence { get; set; }
			public DateTime Time { get; set; }
		}
	}
}