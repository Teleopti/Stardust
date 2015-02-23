using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherenceDetailsViewModelBuilder
	{
		IEnumerable<AdherenceDetailsPercentageModel> Build(Guid personId);
	}

	public class AdherenceDetailsViewModelBuilderViewModelBuilder : IAdherenceDetailsViewModelBuilder
	{
		private readonly INow _now;
		private readonly IAdherenceDetailsReadModelReader _persister;
		private readonly IUserCulture _culture;
		private readonly IUserTimeZone _timeZone;
		private readonly CalculateAdherencePercent _calculateAdherencePercent;

		public AdherenceDetailsViewModelBuilderViewModelBuilder(INow now, IAdherenceDetailsReadModelReader persister, IUserCulture culture, IUserTimeZone timeZone)
		{
			_now = now;
			_persister = persister;
			_culture = culture;
			_timeZone = timeZone;
			_calculateAdherencePercent = new CalculateAdherencePercent(_now);
		}

		public IEnumerable<AdherenceDetailsPercentageModel> Build(Guid personId)
		{
			var readModel = _persister.Read(personId, new DateOnly(_now.UtcDateTime()));
			var result = new List<AdherenceDetailsPercentageModel>();
			if (readModel == null) return result;
			var detailModels = readModel.Model.Activities;
			for (var i = 0; i < detailModels.Count(); i++)
			{
				var detail = detailModels.ElementAt(i);
				result.Add(new AdherenceDetailsPercentageModel
				{
					Name = detail.Name,
					StartTime = convertToAgentTimeZoneAndFormatTimestamp(detail.StartTime),
					ActualStartTime = convertToAgentTimeZoneAndFormatTimestamp(detail.ActualStartTime),
					AdherencePercent =
						(int)_calculateAdherencePercent.ForActivity(readModel.Model, detail, isActivityEnded(i, detailModels.Count(), readModel.Model.ShiftEndTime.HasValue), readModel.Model.LastAdherence)
								.ValueAsPercent()
				});
			}
			if (readModel.Model.ShiftEndTime.HasValue)
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
				return string.Empty;
			var localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			return localTimestamp.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern);
		}

	}
}