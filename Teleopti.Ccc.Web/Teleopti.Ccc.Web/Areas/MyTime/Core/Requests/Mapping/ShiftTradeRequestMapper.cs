﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestMapper : IShiftTradeRequestMapper
	{
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftExchangeOfferRepository _shiftExchangeOfferRepository;

		public ShiftTradeRequestMapper(IPersonRepository personRepository, ILoggedOnUser loggedOnUser, IShiftExchangeOfferRepository shiftExchangeOfferRepository)
		{
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
			_shiftExchangeOfferRepository = shiftExchangeOfferRepository;
		}

		public IPersonRequest Map(ShiftTradeRequestForm form)
		{
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var personTo = _personRepository.Get(form.PersonToId);
			var shiftTradeSwapDetailList = new List<IShiftTradeSwapDetail>();
			var offer = form.ShiftExchangeOfferId != null ? _shiftExchangeOfferRepository.Get(form.ShiftExchangeOfferId) : null;
			foreach (var date in form.Dates)
			{
				var calendarDate = new DateOnly(new DateTime(date.Year, date.Month, date.Day, CultureInfo.CurrentCulture.Calendar));
				var shiftTradeSwapDetail = new ShiftTradeSwapDetail(loggedOnUser, personTo, calendarDate, calendarDate);
				shiftTradeSwapDetailList.Add(shiftTradeSwapDetail);
			}

			var shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetailList){Offer = offer};
			var ret = new PersonRequest(loggedOnUser) { Request = shiftTradeRequest, Subject = form.Subject};
			ret.TrySetMessage(form.Message);
			return ret;
		}
	}
}