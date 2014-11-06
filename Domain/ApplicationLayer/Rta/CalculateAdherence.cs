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
		private readonly IUserCulture _culture;
		private readonly IUserTimeZone _timeZone;
		private readonly HistoricalAdherence _historicalAdherence;

		public CalculateAdherence(
			IAdherencePercentageReadModelPersister adherencePercentageReadModelPersister,
			INow now,
			IUserCulture culture, IUserTimeZone timeZone)
		{
			_adherencePercentageReadModelPersister = adherencePercentageReadModelPersister;
			_now = now;
			_culture = culture;
			_timeZone = timeZone;
			_historicalAdherence = new HistoricalAdherence(_now);
		}

		public AdherencePercentageModel ForToday(Guid personId)
		{
			var readModel = _adherencePercentageReadModelPersister.Get(new DateOnly(_now.UtcDateTime()), personId);

			if (readModel == null || !isValid(readModel))
				return null;

			return new AdherencePercentageModel
			       {
					   LastTimestamp = convertToAgentTimeZoneAndFormatTimestamp(readModel.LastTimestamp),
				       AdherencePercent = (int)_historicalAdherence.ForDay(readModel).ValueAsPercent()
			       };
		}

		private string convertToAgentTimeZoneAndFormatTimestamp(DateTime timestamp)
		{
			var localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(timestamp, _timeZone.TimeZone());
			return localTimestamp.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern);
		}

		private static bool isValid(AdherencePercentageReadModel readModel)
		{
			return !(readModel.TimeInAdherence == TimeSpan.Zero && readModel.TimeOutOfAdherence == TimeSpan.Zero);
		}
	}

	public class AdherencePercentageModel
	{
		public string LastTimestamp { get; set; }
		public int AdherencePercent { get; set; }
	}
}