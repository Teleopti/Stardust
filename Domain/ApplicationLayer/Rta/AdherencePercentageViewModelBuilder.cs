using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherencePercentageViewModelBuilder
	{
		AdherencePercentageViewModel Build(Guid personId);
	}

	public class AdherencePercentageViewModelBuilder : IAdherencePercentageViewModelBuilder
	{
		private readonly IAdherencePercentageReadModelReader _adherencePercentageReadModelReader;
		private readonly INow _now;
		private readonly IUserCulture _culture;
		private readonly IUserTimeZone _timeZone;
		private readonly ICurrentBelongsToDate _date;

		public AdherencePercentageViewModelBuilder(
			INow now, 
			IAdherencePercentageReadModelReader adherencePercentageReadModelReader, 
			IUserCulture culture, 
			IUserTimeZone timeZone, 
			ICurrentBelongsToDate date
			)
		{
			_adherencePercentageReadModelReader = adherencePercentageReadModelReader;
			_now = now;
			_culture = culture;
			_timeZone = timeZone;
			_date = date;
		}

		public AdherencePercentageViewModel Build(Guid personId)
		{
			var readModel = _adherencePercentageReadModelReader.Read(_date.ForPerson(personId), personId);
			if (readModel == null)
				return new AdherencePercentageViewModel();

			return new AdherencePercentageViewModel
			       {
					   LastTimestamp = convertToAgentTimeZoneAndFormatTimestamp(readModel.LastTimestamp),
				       AdherencePercent = percent(readModel)
			       };
		}

		private int percent(AdherencePercentageReadModel model)
		{
			var secondsInAdherence = Convert.ToDouble(model.TimeInAdherence.TotalSeconds);
			var secondsOutOfAdherence = Convert.ToDouble(model.TimeOutOfAdherence.TotalSeconds);

			if (!model.ShiftHasEnded && model.IsLastTimeInAdherence.HasValue)
			{
				var isLastInAdherence = model.IsLastTimeInAdherence.Value;
				var lastTimestamp = model.LastTimestamp ?? DateTime.MinValue;
				var secondsFromLastUpdate = _now.UtcDateTime().Subtract(lastTimestamp).TotalSeconds;
				if (isLastInAdherence)
					secondsInAdherence += secondsFromLastUpdate;
				else
					secondsOutOfAdherence += secondsFromLastUpdate;
			}

			var total = secondsInAdherence + secondsOutOfAdherence;

			return (int) (secondsInAdherence/total*100);
		}

		private string convertToAgentTimeZoneAndFormatTimestamp(DateTime? timestamp)
		{
			if (!timestamp.HasValue)
				return string.Empty;
			var localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			return localTimestamp.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern);
		}

	}

	public class AdherencePercentageViewModel
	{
		public string LastTimestamp { get; set; }
		public int? AdherencePercent { get; set; }
	}

}