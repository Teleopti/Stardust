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

		public AdherenceDetailsViewModelBuilderViewModelBuilder(INow now, IAdherenceDetailsReadModelReader persister, IUserCulture culture, IUserTimeZone timeZone)
		{
			_now = now;
			_persister = persister;
			_culture = culture;
			_timeZone = timeZone;
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
					StartTime = formatToUserTimeZone(detail.StartTime),
					ActualStartTime = formatToUserTimeZone(detail.ActualStartTime),
					AdherencePercent =
						(int) forActivity(readModel.Model, detail, isActivityEnded(i, detailModels.Count(), readModel.Model.ShiftEndTime.HasValue), readModel.Model.LastAdherence)
								.ValueAsPercent()
				});
			}
			if (readModel.Model.ShiftEndTime.HasValue)
			{
				result.Add(new AdherenceDetailsPercentageModel
				{
					Name = UserTexts.Resources.End,
					StartTime = formatToUserTimeZone(readModel.Model.ShiftEndTime),
					ActualStartTime = formatToUserTimeZone(readModel.Model.ActualEndTime)
				});
			}
			return result;
		}

		private Percent forActivity(AdherenceDetailsModel model, ActivityAdherence detail, bool activityEnded, bool isInAdherence)
		{
			var secondsInAdherence = Convert.ToDouble(detail.TimeInAdherence.TotalSeconds);
			var secondsOutOfAdherence = Convert.ToDouble(detail.TimeOutOfAdherence.TotalSeconds);
			if (!activityEnded)
			{
				var lastTimestamp = model.LastUpdate ?? DateTime.MinValue;
				var secondsFromLastUpdate = _now.UtcDateTime().Subtract(lastTimestamp).TotalSeconds;
				if (isInAdherence)
					secondsInAdherence += secondsFromLastUpdate;
				else
					secondsOutOfAdherence += secondsFromLastUpdate;
			}
			var total = secondsInAdherence + secondsOutOfAdherence;

			return new Percent(secondsInAdherence / total);
		}

		private static bool isActivityEnded(int modelIndex, int totalActivites, bool hasShiftEnded)
		{
			return modelIndex < totalActivites - 1 || hasShiftEnded;
		}

		private string formatToUserTimeZone(DateTime? timestamp)
		{
			if (!timestamp.HasValue)
				return string.Empty;
			var userTime = TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			return userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern);
		}

	}
}