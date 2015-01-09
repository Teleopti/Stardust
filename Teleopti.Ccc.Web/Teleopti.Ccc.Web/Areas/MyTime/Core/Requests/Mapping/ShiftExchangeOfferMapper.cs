using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftExchangeOfferMapper : IShiftExchangeOfferMapper
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ILoggedOnUser _loggedOnUser;
		private IPerson _currentPerson;

		public ShiftExchangeOfferMapper(ILoggedOnUser loggedOnUser, IScheduleRepository scheduleRepository, ICurrentScenario currentScenario, 
			ICurrentTeleoptiPrincipal principal)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleRepository = scheduleRepository;
			_currentScenario = currentScenario;
			_principal = principal;
		}

		public IPersonRequest Map (ShiftExchangeOfferForm form, IPersonRequest personRequest)
		{
			_currentPerson = _loggedOnUser.CurrentUser();
			var offer = personRequest.Request as ShiftExchangeOffer;
			if (offer!=null)
			{
				mapOffer(form, personRequest, offer.Status);	
			}
			return personRequest;
		}

		public IPersonRequest Map (ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status)
		{
			_currentPerson = _loggedOnUser.CurrentUser();
			IPersonRequest ret = new PersonRequest(_currentPerson);
			mapOffer (form, ret, status);
			ret.Subject = UserTexts.Resources.ShiftExchangeAnnouncement;
			ret.TrySetMessage("");
			return ret;
		}

		private void mapOffer(ShiftExchangeOfferForm form, IPersonRequest request, ShiftExchangeOfferStatus status)
		{
			var date = new DateOnly(form.Date);
			var schedule = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(_currentPerson,
					new ScheduleDictionaryLoadOptions(false, false) { LoadDaysAfterLeft = false, LoadNotes = false, LoadRestrictions = false },
					new DateOnlyPeriod(date, date), _currentScenario.Current());
			var offer = new ShiftExchangeOffer(schedule[_currentPerson].ScheduledDay(date),
				new ShiftExchangeCriteria(new DateOnly(form.OfferValidTo), createOptionalDateTimePeriod(form, date)), status);
			request.Request = offer;
		}

		private DateTimePeriod? createOptionalDateTimePeriod(ShiftExchangeOfferForm form, DateOnly date)
		{
			if (form.StartTime.HasValue && form.EndTime.HasValue)
				return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.Add(form.StartTime.Value),
					date.Date.Add(form.EndTime.Value.Add((form.EndTimeNextDay ? TimeSpan.FromDays(1) : TimeSpan.Zero))),
					_principal.Current().Regional.TimeZone);

			return null;
		}
	}
}