using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftExchangeOfferMapper : IShiftExchangeOfferMapper
	{
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private IPerson _currentPerson;

		public ShiftExchangeOfferMapper(ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
		}

		public IPersonRequest Map(ShiftExchangeOfferForm form, IPersonRequest personRequest)
		{
			_currentPerson = _loggedOnUser.CurrentUser();
			if (personRequest.Request is ShiftExchangeOffer offer)
			{
				mapOffer(form, personRequest, offer.Status);
			}
			return personRequest;
		}

		public IPersonRequest Map(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status)
		{
			_currentPerson = _loggedOnUser.CurrentUser();
			IPersonRequest ret = new PersonRequest(_currentPerson);
			mapOffer(form, ret, status);
			ret.Subject = UserTexts.Resources.ShiftExchangeAnnouncement;
			ret.TrySetMessage("");
			return ret;
		}

		private void mapOffer(ShiftExchangeOfferForm form, IPersonRequest request, ShiftExchangeOfferStatus status)
		{
			var date = new DateOnly(form.Date);
			var options = new ScheduleDictionaryLoadOptions(false, false)
			{
				LoadDaysAfterLeft = false
			};
			var schedule = _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date), options);

			var timePeriod = (form.WishShiftType == (int) ShiftExchangeLookingForDay.WorkingShift)
				? createOptionalDateTimePeriod(form, date)
				: null;
			var dayFilterCriteria = new ScheduleDayFilterCriteria(form.WishShiftType, timePeriod);
			var criteria = new ShiftExchangeCriteria(new DateOnly(form.OfferValidTo), dayFilterCriteria);
			var offer = new ShiftExchangeOffer(schedule.First(), criteria, status);
			request.Request = offer;
		}

		private DateTimePeriod? createOptionalDateTimePeriod(ShiftExchangeOfferForm form, DateOnly date)
		{
			if (!form.StartTime.HasValue || !form.EndTime.HasValue)
			{
				return null;
			}

			var localStartTime = date.Date.Add(form.StartTime.Value);
			var endTimeSpan = form.EndTime.Value.Add(form.EndTimeNextDay ? TimeSpan.FromDays(1) : TimeSpan.Zero);
			var localEndTime = date.Date.Add(endTimeSpan);
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localStartTime, localEndTime, timezone);
		}
	}
}
