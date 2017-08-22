using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Core;
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
		private readonly ITeamsProvider _teamsProvider;
		private readonly IToggleManager _toggleManager;

		public RequestsController(IRequestsViewModelFactory requestsViewModelFactory,
			IRequestCommandHandlingProvider commandHandlingProvider,
			ILoggedOnUser loggedOnUser, IShiftTradeRequestViewModelFactory shiftTradeRequestViewModelFactory, ITeamsProvider teamsProvider, IToggleManager toggleManager)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
			_commandHandlingProvider = commandHandlingProvider;
			_loggedOnUser = loggedOnUser;
			_shiftTradeRequestViewModelFactory = shiftTradeRequestViewModelFactory;
			_teamsProvider = teamsProvider;
			_toggleManager = toggleManager;
		}

		[HttpPost, Route("api/Requests/requests"), UnitOfWork]
		public virtual RequestListViewModel<AbsenceAndTextRequestViewModel> GetRequests([FromBody]AllRequestsFormData input)
		{
			return _requestsViewModelFactory.CreateAbsenceAndTextRequestListViewModel(input);
		}

		[HttpPost, Route("api/Requests/shiftTradeRequests"), UnitOfWork]
		public virtual RequestListViewModel<ShiftTradeRequestViewModel> GetShiftTradeRequests([FromBody]AllRequestsFormData input)
		{
			return _shiftTradeRequestViewModelFactory.CreateRequestListViewModel(input);
		}

		[HttpPost, Route("api/Requests/overtimeRequests"), UnitOfWork]
		public virtual RequestListViewModel<OvertimeRequestViewModel> GetOvertimeRequests([FromBody]AllRequestsFormData input)
		{
			return _requestsViewModelFactory.CreateOvertimeRequestListViewModel(input);
		}

		[HttpPost, Route("api/Requests/approveRequests"), UnitOfWork]
		public virtual RequestCommandHandlingResult ApproveRequests(RequestsCommandInput input)
		{
			return _commandHandlingProvider.ApproveRequests(input.SelectedRequestIds, input.ReplyMessage);
		}

		[HttpPost, Route("api/Requests/approveWithValidators"), UnitOfWork]
		public virtual RequestCommandHandlingResult ApproveWithValidators(ApproveRequestsWithValidatorInput input)
		{
			return _commandHandlingProvider.ApproveWithValidators(input.RequestIds, input.Validators);
		}

		[HttpPost, Route("api/Requests/replyRequests"), UnitOfWork]
		public virtual RequestCommandHandlingResult ReplyRequests(RequestsCommandInput input)
		{
			return _commandHandlingProvider.ReplyRequests(input.SelectedRequestIds, input.ReplyMessage);
		}

		[HttpPost, Route("api/Requests/denyRequests"), UnitOfWork]
		public virtual RequestCommandHandlingResult DenyRequests(RequestsCommandInput input)
		{
			return _commandHandlingProvider.DenyRequests(input.SelectedRequestIds, input.ReplyMessage);
		}

		[HttpPost, Route("api/Requests/cancelRequests"), UnitOfWork]
		public virtual RequestCommandHandlingResult CancelRequests(RequestsCommandInput input)
		{
			return _commandHandlingProvider.CancelRequests(input.SelectedRequestIds, input.ReplyMessage);
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

		[UnitOfWork, HttpGet, Route("api/Requests/FetchPermittedTeamHierachy")]
		public virtual BusinessUnitWithSitesViewModel FetchPermittedTeamHierachy(DateTime date)
		{

			return _teamsProvider.GetPermittedTeamHierachy(new DateOnly(date), DefinedRaptorApplicationFunctionPaths.WebRequests);
		}

		[UnitOfWork, HttpGet, Route("api/Requests/GetOrganizationWithPeriod")]
		public virtual BusinessUnitWithSitesViewModel GetOrganizationWithPeriod(DateTime startDate,DateTime endDate)
		{
			return _toggleManager.IsEnabled(Toggles.Wfm_FetchBusinessHierarchyFromReadModel_45275)
				? _teamsProvider.GetOrganizationWithPeriod(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
					DefinedRaptorApplicationFunctionPaths.WebRequests)
				: _teamsProvider.GetOrganizationBasedOnRawData(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
					DefinedRaptorApplicationFunctionPaths.WebRequests);

		}
	}
}