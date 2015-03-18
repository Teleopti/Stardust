using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface IAdherenceDetailsViewModelBuilder
	{
		IEnumerable<AdherenceDetailViewModel> Build(Guid personId);
	}

	public class AdherenceDetailsViewModelBuilder : IAdherenceDetailsViewModelBuilder
	{
		private readonly INow _now;
		private readonly IAdherenceDetailsReadModelReader _persister;
		private readonly IUserCulture _culture;
		private readonly IUserTimeZone _timeZone;
		private readonly ICurrentBelongsToDate _date;

		public AdherenceDetailsViewModelBuilder(
			INow now, 
			IAdherenceDetailsReadModelReader persister, 
			IUserCulture culture, 
			IUserTimeZone timeZone,
			ICurrentBelongsToDate date)
		{
			_now = now;
			_persister = persister;
			_culture = culture;
			_timeZone = timeZone;
			_date = date;
		}

		public IEnumerable<AdherenceDetailViewModel> Build(Guid personId)
		{
			var model = _persister.Read(personId, _date.ForPerson(personId));
			if (model == null) return new AdherenceDetailViewModel[] {};

			var activities = from a in model.Model.Activities
				select new AdherenceDetailViewModel
				{
					Name = a.Name,
					StartTime = formatToUserTimeZone(a.StartTime),
					ActualStartTime = formatToUserTimeZone(a.ActualStartTime),
					AdherencePercent = percent(model.Model, a)
				};
			if (model.Model.ShiftEndTime.HasValue)
				activities = activities.Append(new AdherenceDetailViewModel
				{
					Name = UserTexts.Resources.End,
					StartTime = formatToUserTimeZone(model.Model.ShiftEndTime),
					ActualStartTime = formatToUserTimeZone(model.Model.ActualEndTime)
				});
			return activities.ToArray();
		}

		private int? percent(AdherenceDetailsModel model, ActivityAdherence activity)
		{
			var activityEnded = model.ShiftEndTime.HasValue || !model.Activities.Last().Equals(activity);
			if (!activity.TimeInAdherence.HasValue && !activity.TimeOutOfAdherence.HasValue) 
				return null;

			var secondsInAdherence = Convert.ToDouble(activity.TimeInAdherence.GetValueOrDefault().TotalSeconds);
			var secondsOutOfAdherence = Convert.ToDouble(activity.TimeOutOfAdherence.GetValueOrDefault().TotalSeconds);
			if (!activityEnded)
			{
				var lastTimestamp = model.LastUpdate ?? DateTime.MinValue;
				var secondsFromLastUpdate = _now.UtcDateTime().Subtract(lastTimestamp).TotalSeconds;
				if (model.LastAdherence.HasValue)
				{
					if (model.LastAdherence.Value)
						secondsInAdherence += secondsFromLastUpdate;
					else
						secondsOutOfAdherence += secondsFromLastUpdate;
				}
			}
			var total = secondsInAdherence + secondsOutOfAdherence;

			return (int) (secondsInAdherence/total*100);
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