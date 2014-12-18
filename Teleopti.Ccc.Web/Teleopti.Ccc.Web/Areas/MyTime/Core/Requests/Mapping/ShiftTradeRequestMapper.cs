using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestMapper : IShiftTradeRequestMapper
	{
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftExchangeOfferRepository _shiftExchangeOfferRepository;
		private readonly IScheduleProvider _scheduleProvider;

		public ShiftTradeRequestMapper(IPersonRepository personRepository, ILoggedOnUser loggedOnUser, IShiftExchangeOfferRepository shiftExchangeOfferRepository, IScheduleProvider scheduleProvider)
		{
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
			_shiftExchangeOfferRepository = shiftExchangeOfferRepository;
			_scheduleProvider = scheduleProvider;
		}

		public IPersonRequest Map(ShiftTradeRequestForm form)
		{
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var personTo = _personRepository.Get(form.PersonToId);
			var shiftTradeSwapDetailList = new List<IShiftTradeSwapDetail>();
			var offer = form.ShiftExchangeOfferId != null ? _shiftExchangeOfferRepository.Get(form.ShiftExchangeOfferId.Value) : null;
			foreach (var date in form.Dates)
			{
				var calendarDate = new DateOnly(new DateTime(date.Year, date.Month, date.Day, CultureInfo.CurrentCulture.Calendar));
				var shiftTradeSwapDetail = new ShiftTradeSwapDetail(loggedOnUser, personTo, calendarDate, calendarDate);
				shiftTradeSwapDetailList.Add(shiftTradeSwapDetail);
			}

			var shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetailList);
			IPersonRequest ret;
			if (offer == null)
			{
				ret = new PersonRequest(loggedOnUser) {Request = shiftTradeRequest};
			}
			else
			{
				var scheduleDays = _scheduleProvider.GetScheduleForPersons(form.Dates.SingleOrDefault(), new[] { loggedOnUser });
				ret = offer.MakeShiftTradeRequest(scheduleDays.SingleOrDefault());
			}
			ret.Subject = form.Subject;
			ret.TrySetMessage(form.Message);
			return ret;
		}
	}
}