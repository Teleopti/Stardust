using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftExchangeOfferMapper : IShiftExchangeOfferMapper
	{
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftExchangeOfferMapper(ILoggedOnUser loggedOnUser, IScheduleRepository scheduleRepository, ICurrentScenario currentScenario, 
			ICurrentTeleoptiPrincipal principal)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleRepository = scheduleRepository;
			_currentScenario = currentScenario;
			_principal = principal;
		}

		public IPersonRequest Map(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status)
		{
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var date = new DateOnly(form.Date);
			var schedule =
				_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(loggedOnUser,
					new ScheduleDictionaryLoadOptions(false, false) { LoadDaysAfterLeft = false, LoadNotes = false, LoadRestrictions = false },
					new DateOnlyPeriod(date, date), _currentScenario.Current());
			var offer = new ShiftExchangeOffer(schedule[loggedOnUser].ScheduledDay(date),
				new ShiftExchangeCriteria(new DateOnly(form.OfferValidTo), createOptionalDateTimePeriod(form, date)), status);

			IPersonRequest ret = new PersonRequest(loggedOnUser) {Request = offer};

			ret.Subject = "Shift Exchange Announcement";
			return ret;
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