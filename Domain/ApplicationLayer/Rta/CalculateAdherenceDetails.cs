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
		private readonly IAdherenceDetailsReadModelPersister _persister;
		private readonly IUserCulture _culture;
		private readonly IUserTimeZone _timeZone;
		private readonly CalculateAdherencePercent _calculateAdherencePercent;

		public CalculateAdherenceDetails(INow now, IAdherenceDetailsReadModelPersister persister, IUserCulture culture, IUserTimeZone timeZone)
		{
			_now = now;
			_persister = persister;
			_culture = culture;
			_timeZone = timeZone;
			_calculateAdherencePercent = new CalculateAdherencePercent(_now);
		}

		public IEnumerable<AdherenceDetailsPercentageModel> ForDetails(Guid personId)
		{
			var readModel = _persister.Get(personId, new DateOnly(_now.UtcDateTime()));
			var result = new List<AdherenceDetailsPercentageModel>();
			if (readModel == null) return result;
			var detailModels = readModel.Model.Details;
			for (var i = 0; i < detailModels.Count; i++)
			{
				if (detailModels[i] == null || !isValid(detailModels[i]))
					continue;
				result.Add(new AdherenceDetailsPercentageModel
				{
					Name = detailModels[i].Name,
					StartTime = convertToAgentTimeZoneAndFormatTimestamp(detailModels[i].StartTime),
					ActualStartTime = convertToAgentTimeZoneAndFormatTimestamp(detailModels[i].ActualStartTime),
					AdherencePercent =
						(int)_calculateAdherencePercent.ForActivity(detailModels[i],isActivityEnded(i, detailModels.Count, readModel.Model.HasShiftEnded), readModel.Model.IsInAdherence)
								.ValueAsPercent()
				});
			}
			if (readModel.Model.HasShiftEnded)
			{
				result.Add(new AdherenceDetailsPercentageModel
				{
					Name = UserTexts.Resources.End,
					StartTime = convertToAgentTimeZoneAndFormatTimestamp(readModel.Model.ShiftEndTime),
					ActualStartTime = convertToAgentTimeZoneAndFormatTimestamp(readModel.Model.ActualEndTime)
				});
			}
			return result;
		}

		private static bool isActivityEnded(int modelIndex, int totalActivites, bool hasShiftEnded)
		{
			return modelIndex < totalActivites - 1 || hasShiftEnded;
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


		private static bool isValid(AdherenceDetailModel detailModel)
		{
			return detailModel.StartTime.HasValue;
		}
	}
}