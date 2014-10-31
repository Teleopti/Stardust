using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ICalculateAdherence
	{
		AdherenceInfo ForToday(Guid personId);
	}

	public class CalculateAdherence : ICalculateAdherence
	{
		private readonly IAdherencePercentageReadModelPersister _adherencePercentageReadModelPersister;
		private readonly INow _now;
		private readonly HistoricalAdherence _historicalAdherence;

		public CalculateAdherence(IAdherencePercentageReadModelPersister adherencePercentageReadModelPersister, INow now)
		{
			_adherencePercentageReadModelPersister = adherencePercentageReadModelPersister;
			_now = now;
			_historicalAdherence = new HistoricalAdherence(_now);
		}

		public AdherenceInfo ForToday(Guid personId)
		{
			var readModel = _adherencePercentageReadModelPersister.Get(new DateOnly(_now.UtcDateTime()), personId);

			if (readModel == null || !isValid(readModel))
			{
				return new AdherenceInfo() { IsValid = false };
			}

			return new AdherenceInfo
			       {
				       MinutesInAdherence = readModel.MinutesInAdherence,
				       MinutesOutOfAdherence = readModel.MinutesOutOfAdherence,
				       LastTimestamp = readModel.LastTimestamp,
				       AdherencePercent = (int)_historicalAdherence.ForDay(readModel).ValueAsPercent(),
				       IsValid = true
			       };
		}

		private static bool isValid(AdherencePercentageReadModel readModel)
		{
			return !(readModel.MinutesInAdherence == 0 && readModel.MinutesOutOfAdherence == 0);
		}
	}

	public class AdherenceInfo
	{
		public int MinutesInAdherence { get; set; }
		public int MinutesOutOfAdherence { get; set; }
		public DateTime LastTimestamp { get; set; }
		public int AdherencePercent { get; set; }
		public bool IsValid { get; set; }
	}
}