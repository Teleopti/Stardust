using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestMapper : IShiftTradeRequestMapper
	{
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IScheduleProvider _scheduleProvider;

		public ShiftTradeRequestMapper(IPersonRepository personRepository, ILoggedOnUser loggedOnUser, IPersonRequestRepository personRequestRepository, IScheduleProvider scheduleProvider)
		{
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_personRequestRepository = personRequestRepository;
		}

		public IPersonRequest Map(ShiftTradeRequestForm form)
		{
			var loggedOnUser = _loggedOnUser.CurrentUser();
			var personTo = _personRepository.Get(form.PersonToId);
			var shiftTradeSwapDetailList = new List<IShiftTradeSwapDetail>();
			IShiftExchangeOffer offer = null;
			if (form.ShiftExchangeOfferId != null)
			{
				var personRequest = _personRequestRepository.FindPersonRequestByRequestId(form.ShiftExchangeOfferId.Value);
				offer = personRequest.Request as IShiftExchangeOffer;
			}
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