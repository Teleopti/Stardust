using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebRequests)]
	public class RequestsController : ApiController
	{
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly IRequestCommandHandlingProvider _commandHandlingProvider;
		private readonly IShiftTradeRequestViewModelFactory _shiftTradeRequestViewModelFactory;
		private readonly ILoggedOnUser _loggedOnUser;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory,

			IRequestCommandHandlingProvider commandHandlingProvider,
			ILoggedOnUser loggedOnUser, IShiftTradeRequestViewModelFactory shiftTradeRequestViewModelFactory)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
			_commandHandlingProvider = commandHandlingProvider;
			_loggedOnUser = loggedOnUser;
			_shiftTradeRequestViewModelFactory = shiftTradeRequestViewModelFactory;
		}

		[HttpPost, Route("api/Requests/loadTextAndAbsenceRequests"), UnitOfWork]
		public virtual IEnumerable<RequestViewModel> AllRequests(AllRequestsFormData input)
		{
			return _requestsViewModelFactory.Create(input);
		}
		
		[HttpGet, Route("api/Requests/requests"), UnitOfWork]
		public virtual RequestListViewModel GetRequests([ModelBinder(typeof (AllRequestsFormDataConverter))] AllRequestsFormData input)
		{
			return _requestsViewModelFactory.CreateRequestListViewModel(input);
		}

		[HttpGet, Route("api/Requests/shiftTradeRequests"), UnitOfWork]
		public virtual RequestListViewModel GetShiftTradeRequests([ModelBinder(typeof(AllRequestsFormDataConverter))] AllRequestsFormData input)
		{
			return _shiftTradeRequestViewModelFactory.CreateRequestListViewModel(input);
		}

		[HttpPost, Route("api/Requests/approveRequests"), UnitOfWork]
		public virtual RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> requestIds)
		{
			return _commandHandlingProvider.ApproveRequests(requestIds);
		}

		[HttpPost, Route("api/Requests/approveBasedOnBudget"), UnitOfWork]
		public virtual RequestCommandHandlingResult ApproveRequestsBasedOnBudgetAllotment(IEnumerable<Guid> requestIds)
		{
			return _commandHandlingProvider.ApproveRequestsBasedOnBudgetAllotment(requestIds);
		}

		[HttpPost, Route("api/Requests/denyRequests"), UnitOfWork]
		public virtual RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> requestIds)
		{
			return _commandHandlingProvider.DenyRequests(requestIds);
		}

		[HttpPost, Route("api/Requests/cancelRequests"), UnitOfWork]
		public virtual RequestCommandHandlingResult CancelRequests(IEnumerable<Guid> requestIds)
		{
			return _commandHandlingProvider.CancelRequests(requestIds);
		}

		[HttpGet, Route("api/Requests/runWaitlist"), UnitOfWork]
		public virtual RequestCommandHandlingResult RunRequestWaitlist(DateTime startTime, DateTime endTime)
		{
			var timezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var startTimeUtc = timezone.SafeConvertTimeToUtc(startTime);
			var endTimeUtc = timezone.SafeConvertTimeToUtc(endTime);
			var period = new DateTimePeriod(startTimeUtc, endTimeUtc);
			return _commandHandlingProvider.RunWaitlist(period);
		}
	}
}