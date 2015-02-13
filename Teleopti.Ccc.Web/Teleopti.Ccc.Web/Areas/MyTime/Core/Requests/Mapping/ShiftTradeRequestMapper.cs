using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestMapper : IShiftTradeRequestMapper
	{
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IShiftTradeRequestProvider _shiftTradeRequestprovider;

		public ShiftTradeRequestMapper(IPersonRepository personRepository, ILoggedOnUser loggedOnUser, IPersonRequestRepository personRequestRepository, IScheduleProvider scheduleProvider, IShiftTradeRequestProvider shiftTradeRequestprovider)
		{
			_personRepository = personRepository;
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_shiftTradeRequestprovider = shiftTradeRequestprovider;
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
				HandleLockTrading(offer);
			}
			ret.Subject = form.Subject;
			ret.TrySetMessage(form.Message);
			return ret;
		}

		private void HandleLockTrading(IShiftExchangeOffer offer)
		{
			var workflowControlSet = _shiftTradeRequestprovider.RetrieveUserWorkflowControlSet();
			if (workflowControlSet.LockTrading && !workflowControlSet.AutoGrantShiftTradeRequest)
			{
				offer.Status = ShiftExchangeOfferStatus.PendingAdminApproval;
			}
		}
	}
}