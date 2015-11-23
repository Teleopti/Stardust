using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders
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
					   LastTimestamp = currentTimeInLoggedOnUserTimeZone(),
				       AdherencePercent = percent(readModel)
			       };
		}

		private int? percent(AdherencePercentageReadModel model)
		{
			var secondsInAdherence = model.TimeInAdherence.TotalSeconds;
			var secondsOutOfAdherence = model.TimeOutOfAdherence.TotalSeconds;

			var ongoingShift = !model.ShiftHasEnded;
			var inNeutralAdherence = !model.IsLastTimeInAdherence.HasValue;
			if (ongoingShift && !inNeutralAdherence)
			{
				var secondsFromLastUpdate = _now.UtcDateTime()
					.Subtract(model.LastTimestamp.GetValueOrDefault())
					.TotalSeconds;
				if (model.IsLastTimeInAdherence.GetValueOrDefault(false))
					secondsInAdherence += secondsFromLastUpdate;
				else
					secondsOutOfAdherence += secondsFromLastUpdate;
			}

			var total = secondsInAdherence + secondsOutOfAdherence;
			if (total.Equals(0))
				return null;

			return (int) (secondsInAdherence/total*100);
		}

		private string currentTimeInLoggedOnUserTimeZone()
		{
			var localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			return localTimestamp.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern);
		}

	}
}