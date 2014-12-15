using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftExchangeOfferPersister : IShiftExchangeOfferPersister
	{
		private readonly IShiftExchangeOfferRepository _shiftExchangeOfferRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly ICurrentScenario _currentScenario;

		public ShiftExchangeOfferPersister(IShiftExchangeOfferRepository shiftExchangeOfferRepository, IScheduleRepository scheduleRepository, IPersonRepository personRepository, ICurrentTeleoptiPrincipal principal, ICurrentScenario currentScenario)
		{
			_shiftExchangeOfferRepository = shiftExchangeOfferRepository;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_principal = principal;
			_currentScenario = currentScenario;
		}

		public Guid Persist(ShiftExchangeOfferForm form, ShiftExchangeOfferStatus status)
		{
			var person = _principal.Current().GetPerson(_personRepository);
			var date = new DateOnly(form.Date);
			var schedule =
				_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false,false) {LoadDaysAfterLeft = false, LoadNotes = false, LoadRestrictions = false},
					new DateOnlyPeriod(date, date), _currentScenario.Current());
			var offer = new ShiftExchangeOffer(schedule[person].ScheduledDay(date),
				new ShiftExchangeCriteria(new DateOnly(form.OfferValidTo), createOptionalDateTimePeriod(form, date)), status);

			_shiftExchangeOfferRepository.Add(offer);
			return offer.Id.GetValueOrDefault();
		}		
		
		public Guid Persist(ShiftExchangeOffer offer)
		{
			_shiftExchangeOfferRepository.Add(offer);
			return offer.Id.GetValueOrDefault();
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