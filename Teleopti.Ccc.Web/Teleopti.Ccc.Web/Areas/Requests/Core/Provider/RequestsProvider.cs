﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class RequestsProvider : IRequestsProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly IShiftTradeSwapScheduleDetailsMapper _shiftTradeSwapScheduleDetailsMapper;

		public RequestsProvider(IPersonRequestRepository repository, IPermissionProvider permissionProvider, IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker, IShiftTradeSwapScheduleDetailsMapper shiftTradeSwapScheduleDetailsMapper )
		{
			_repository = repository;
			_permissionProvider = permissionProvider;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
			_shiftTradeSwapScheduleDetailsMapper = shiftTradeSwapScheduleDetailsMapper;
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
			var emptyShiftTradeRequestChecker = new EmptyShiftTradeRequestChecker();
			var referredRequests = new List<IPersonRequest>();
			foreach (var request in requests
				.Where (request => request.Request is IShiftTradeRequest)
				.Select (request => request))
			{
				var shiftTradeRequest = (IShiftTradeRequest) request.Request;
				if (request.IsPending || request.IsNew)
				{
					var shiftTradeRequestStatus = shiftTradeRequest.GetShiftTradeStatus (emptyShiftTradeRequestChecker);
					_shiftTradeRequestStatusChecker.Check(shiftTradeRequest);
					if (shiftTradeRequestStatus != ShiftTradeStatus.Referred)
					{
						if (shiftTradeRequest.GetShiftTradeStatus (emptyShiftTradeRequestChecker) == ShiftTradeStatus.Referred)
						{
							referredRequests.Add (request);
						}
					}
				}
				else
				{
					_shiftTradeSwapScheduleDetailsMapper.Map (shiftTradeRequest);
				}

			}

			return requests.Where (request => !referredRequests.Contains (request));
		}
		
		private bool permissionCheckPredicate(IPersonRequest request)
		{
			var requestPerson = request.Person;
			var checkDate = new DateOnly(request.Request.Period.StartDateTimeLocal(requestPerson.PermissionInformation.DefaultTimeZone()));
			ITeam team = requestPerson.MyTeam(checkDate);
			ISite site = team?.Site;
			IBusinessUnit bu = site?.BusinessUnit;
			
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