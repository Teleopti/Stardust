using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ICalculateAdherenceDetails
	{
		IEnumerable<AdherenceDetailsPercentageModel> ForDetails(Guid personId);
	}
	public class CalculateAdherenceDetails : ICalculateAdherenceDetails
	{
		private readonly INow _now;
		private readonly IAdherenceDetailsReadModelPersister _readModelPersister;
		private readonly IUserCulture _culture;
		private readonly IUserTimeZone _timeZone;
		private readonly CalculateAdherencePercent _calculateAdherencePercent;

		public CalculateAdherenceDetails(INow now, IAdherenceDetailsReadModelPersister readModelPersister, IUserCulture culture, IUserTimeZone timeZone)
		{
			_now = now;
			_readModelPersister = readModelPersister;
			_culture = culture;
			_timeZone = timeZone;
			_calculateAdherencePercent = new CalculateAdherencePercent(_now);
		}

		public IEnumerable<AdherenceDetailsPercentageModel> ForDetails(Guid personId)
		{
			var readModel = _readModelPersister.Get(personId, new DateOnly(_now.UtcDateTime()));
			var result = new List<AdherenceDetailsPercentageModel>();
			if (readModel == null) return result;
			readModel.Model.DetailModels.ForEach(m =>
			{
				if (m == null || !isValid(m))
					return;
				result.Add(new AdherenceDetailsPercentageModel
				{
					Name = m.Name,
					StartTime = convertToAgentTimeZoneAndFormatTimestamp(m.StartTime),
					ActualStartTime = convertToAgentTimeZoneAndFormatTimestamp(m.ActualStartTime),
					AdherencePercent = (int)_calculateAdherencePercent.ForActivity(m).ValueAsPercent()
				});
			});
			return result;
		}

		private string convertToAgentTimeZoneAndFormatTimestamp(DateTime? timestamp)
		{
			if (!timestamp.HasValue)
			{
				return string.Empty;
			}
			var localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			return localTimestamp.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern);
		}


		private static bool isValid(AdherenceDetailModel readModel)
		{
			return !(readModel.TimeInAdherence == TimeSpan.Zero && readModel.TimeOutOfAdherence == TimeSpan.Zero);
		}
	}
}