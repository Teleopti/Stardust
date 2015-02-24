using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherenceDetailsViewModelBuilder
	{
		IEnumerable<AdherenceDetailViewModel> Build(Guid personId);
	}

	public class AdherenceDetailsViewModelBuilderViewModelBuilder : IAdherenceDetailsViewModelBuilder
	{
		private readonly INow _now;
		private readonly IAdherenceDetailsReadModelReader _persister;
		private readonly IUserCulture _culture;
		private readonly IUserTimeZone _timeZone;
		private readonly IPersonRepository _personRepository;

		public AdherenceDetailsViewModelBuilderViewModelBuilder(
			INow now, 
			IAdherenceDetailsReadModelReader persister, 
			IUserCulture culture, 
			IUserTimeZone timeZone,
			IPersonRepository personRepository)
		{
			_now = now;
			_persister = persister;
			_culture = culture;
			_timeZone = timeZone;
			_personRepository = personRepository;
		}

		public IEnumerable<AdherenceDetailViewModel> Build(Guid personId)
		{
			var model = _persister.Read(personId, getDate(personId));
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

		private DateOnly getDate(Guid personId)
		{
			var person = _personRepository.Get(personId);
			return person != null ? 
				new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), person.PermissionInformation.DefaultTimeZone())) : 
				new DateOnly(_now.UtcDateTime());
		}

		private int percent(AdherenceDetailsModel model, ActivityAdherence activity)
		{
			var activityEnded = model.ShiftEndTime.HasValue || !model.Activities.Last().Equals(activity);
			var secondsInAdherence = Convert.ToDouble(activity.TimeInAdherence.TotalSeconds);
			var secondsOutOfAdherence = Convert.ToDouble(activity.TimeOutOfAdherence.TotalSeconds);
			if (!activityEnded)
			{
				var lastTimestamp = model.LastUpdate ?? DateTime.MinValue;
				var secondsFromLastUpdate = _now.UtcDateTime().Subtract(lastTimestamp).TotalSeconds;
				if (model.LastAdherence)
					secondsInAdherence += secondsFromLastUpdate;
				else
					secondsOutOfAdherence += secondsFromLastUpdate;
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