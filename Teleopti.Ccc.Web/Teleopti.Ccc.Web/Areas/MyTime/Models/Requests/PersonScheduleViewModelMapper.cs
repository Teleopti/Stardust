using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Core.Extensions;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class PersonScheduleViewModelMapper
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;

		public PersonScheduleViewModelMapper(IUserTimeZone userTimeZone, INow now)
		{
			_userTimeZone = userTimeZone;
			_now = now;
		}

		private static DateTime roundUp(DateTime dt, TimeSpan d)
		{
			return new DateTime((dt.Ticks + d.Ticks - 1)/d.Ticks*d.Ticks);
		}

		public PersonScheduleViewModel Map(PersonScheduleData s)
		{
			return new PersonScheduleViewModel
			{
				Name = s.CommonAgentNameSetting.BuildFor(s.Person),
				PersonAbsences = from p in s.PersonAbsences ?? new IPersonAbsence[] {}
					select map(s, p),
				DefaultIntradayAbsenceData = s.Model?.Shift == null ? null : map(s.Model.Shift.Projection),
				TimeZoneName = s.Person.PermissionInformation.DefaultTimeZone().DisplayName,
				IanaTimeZoneLoggedOnUser = s.IanaTimeZoneLoggedOnUser,
				IanaTimeZoneOther = s.IanaTimeZoneOther,
				Activities = (s.Activities ?? new IActivity[0]).Select(a => new PersonScheduleViewModelActivity {Id = a.Id.ToString(),Name = a.Name}),
				Absences = (s.Absences ?? new IAbsence[0]).Select(a => new PersonScheduleViewModelAbsence { Id = a.Id.ToString(), Name = a.Name })
			};
		}

		private string startTime(IList<SimpleLayer> s)
		{
			var firstLayer = s.FirstOrDefault();
			if (firstLayer == null)
				return TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.Zero, CultureInfo.CurrentCulture);
			var now = _now.UtcDateTime();
			if (now >= firstLayer.Start && now <= s.LastOrDefault().End)
				return
					TimeHelper.TimeOfDayFromTimeSpan(
						TimeZoneInfo.ConvertTimeFromUtc(roundUp(now, TimeSpan.FromMinutes(15)), _userTimeZone.TimeZone()).TimeOfDay,
						CultureInfo.CurrentCulture);
			return TimeHelper.TimeOfDayFromTimeSpan(TimeZoneInfo.ConvertTimeFromUtc(firstLayer.Start, _userTimeZone.TimeZone()).TimeOfDay, CultureInfo.CurrentCulture);
		}

		private string endTime(IList<SimpleLayer> s)
		{
			var now = _now.UtcDateTime();
			var firstLayer = s.FirstOrDefault();
			if (firstLayer == null)
				return TimeHelper.TimeOfDayFromTimeSpan(TimeSpan.Zero, CultureInfo.CurrentCulture);
			if (now >= firstLayer.Start && now <= s.LastOrDefault().End)
				return
					TimeHelper.TimeOfDayFromTimeSpan(
						TimeZoneInfo.ConvertTimeFromUtc(s.LastOrDefault().End, _userTimeZone.TimeZone()).TimeOfDay,
						CultureInfo.CurrentCulture);
			return TimeHelper.TimeOfDayFromTimeSpan(TimeZoneInfo.ConvertTimeFromUtc(firstLayer.Start, _userTimeZone.TimeZone()).AddHours(1).TimeOfDay, CultureInfo.CurrentCulture);
		}

		private DefaultIntradayAbsenceViewModel map(IList<SimpleLayer> s)
		{
			return new DefaultIntradayAbsenceViewModel
			{
				StartTime = startTime(s),
				EndTime = endTime(s)
			};
		}

		private PersonScheduleViewModelPersonAbsence map(PersonScheduleData personScheduleData, IPersonAbsence personAbsence)
		{
			var destinationTimeZone = _userTimeZone.TimeZone();
			return new PersonScheduleViewModelPersonAbsence
			{
				Id = personAbsence.Id.GetValueOrDefault().ToString(),
				StartTime =
					TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Layer.Period.StartDateTime, destinationTimeZone)
						.ToFixedDateTimeFormat(),
				EndTime =
					TimeZoneInfo.ConvertTimeFromUtc(personAbsence.Layer.Period.EndDateTime, destinationTimeZone)
						.ToFixedDateTimeFormat(),
				Name = personAbsence.Layer.Payload.Confidential && !personScheduleData.HasViewConfidentialPermission
					? ConfidentialPayloadValues.Description.Name
					: personAbsence.Layer.Payload.Description.Name,
				Color = personAbsence.Layer.Payload.Confidential && !personScheduleData.HasViewConfidentialPermission
					? ConfidentialPayloadValues.DisplayColorHex
					: personAbsence.Layer.Payload.DisplayColor.ToHtml()
			};
		}
	}
}
