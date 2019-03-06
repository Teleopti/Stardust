using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class RequestsProvider : IRequestsProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly IShiftTradeSwapScheduleDetailsMapper _shiftTradeSwapScheduleDetailsMapper;
		private readonly ILoggedOnUser _loggedOnUser;

		public RequestsProvider(IPersonRequestRepository repository, IPermissionProvider permissionProvider, IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker, IShiftTradeSwapScheduleDetailsMapper shiftTradeSwapScheduleDetailsMapper, ILoggedOnUser loggedOnUser)
		{
			_repository = repository;
			_permissionProvider = permissionProvider;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
			_shiftTradeSwapScheduleDetailsMapper = shiftTradeSwapScheduleDetailsMapper;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<IPersonRequest> RetrieveAbsenceAndTextRequests(RequestFilter filter, out int totalCount)
		{
			var requests = _repository.FindAbsenceAndTextRequests(filter, out totalCount).Where(permissionCheckPredicate).ToList();

			return requests;
		}

		public IEnumerable<IPersonRequest> RetrieveShiftTradeRequests(RequestFilter filter, out int totalCount)
		{
			var requests = _repository.FindShiftTradeRequests(filter, out totalCount).Where(permissionCheckPredicate).ToList();

			return setupShiftTradeRequestStatus(requests);
		}

		public IEnumerable<IPersonRequest> RetrieveOvertimeRequests(RequestFilter filter, out int totalCount)
		{
			var requests = _repository.FindOvertimeRequests(filter, out totalCount).Where(permissionCheckPredicate).ToList();

			return requests;
		}

		private IEnumerable<IPersonRequest> setupShiftTradeRequestStatus (IEnumerable<IPersonRequest> requests)
		{
			var referredRequests = new List<IPersonRequest>();
			foreach (var request in requests
				.Where (request => request.Request is IShiftTradeRequest)
				.Select (request => request))
			{
				var shiftTradeRequest = (IShiftTradeRequest) request.Request;
				if (request.IsPending || request.IsNew)
				{
					IShiftTradeRequestStatusChecker checker = new EmptyShiftTradeRequestChecker();
					if (_loggedOnUser.CurrentUser() == shiftTradeRequest.PersonFrom
						|| _loggedOnUser.CurrentUser() == shiftTradeRequest.PersonTo)
						checker = _shiftTradeRequestStatusChecker;
					var shiftTradeRequestStatus = shiftTradeRequest.GetShiftTradeStatus(checker);
					if (shiftTradeRequestStatus == ShiftTradeStatus.Referred) referredRequests.Add(request);
				}
				else
				{
					_shiftTradeSwapScheduleDetailsMapper.Map (shiftTradeRequest);
				}

			}

			return requests.Except(referredRequests);
		}
		
		private bool permissionCheckPredicate(IPersonRequest request)
		{
			var requestPerson = request.Person;
			var checkDate = new DateOnly(request.Request.Period.StartDateTimeLocal(requestPerson.PermissionInformation.DefaultTimeZone()));
			ITeam team = requestPerson.MyTeam(checkDate);
			ISite site = team?.Site;
			IBusinessUnit bu = site?.GetOrFillWithBusinessUnit_DONTUSE();
			
			var authorizeOrganisationDetail = new PersonAuthorization
			{
				PersonId = requestPerson.Id ?? Guid.Empty,
				TeamId = team?.Id,
				SiteId = site?.Id,
				BusinessUnitId = bu?.Id ?? Guid.Empty
			};

			return _permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.WebRequests,
				checkDate,
				authorizeOrganisationDetail);
		}
	}
}