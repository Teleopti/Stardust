using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ICalculateAdherence
	{
		AdherencePercentageModel ForToday(Guid personId);
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

		public AdherencePercentageModel ForToday(Guid personId)
		{
			var readModel = _adherencePercentageReadModelPersister.Get(new DateOnly(_now.UtcDateTime()), personId);

			if (readModel == null || !isValid(readModel))
				return null;

			return new AdherencePercentageModel
			       {
				       LastTimestamp = readModel.LastTimestamp,
				       AdherencePercent = (int)_historicalAdherence.ForDay(readModel).ValueAsPercent()
			       };
		}

		private static bool isValid(AdherencePercentageReadModel readModel)
		{
			return !(readModel.TimeInAdherence == TimeSpan.Zero && readModel.TimeOutOfAdherence == TimeSpan.Zero);
		}
	}

	public class AdherencePercentageModel
	{
		public DateTime LastTimestamp { get; set; }
		public int AdherencePercent { get; set; }
	}
}