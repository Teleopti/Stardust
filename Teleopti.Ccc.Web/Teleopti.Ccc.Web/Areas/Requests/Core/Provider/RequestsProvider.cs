using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class RequestsProvider : IRequestsProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPeopleSearchProvider _peopleSearchProvider;
		private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly IShiftTradeSwapScheduleDetailsMapper _shiftTradeSwapScheduleDetailsMapper;

		public RequestsProvider(IPersonRequestRepository repository, IUserTimeZone userTimeZone, IPermissionProvider permissionProvider, IPeopleSearchProvider peopleSearchProvider, IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker, IShiftTradeSwapScheduleDetailsMapper shiftTradeSwapScheduleDetailsMapper )
		{
			_repository = repository;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
			_peopleSearchProvider = peopleSearchProvider;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
			_shiftTradeSwapScheduleDetailsMapper = shiftTradeSwapScheduleDetailsMapper;
		}

		public IEnumerable<IPersonRequest> RetrieveRequests(AllRequestsFormData input, IEnumerable<RequestType> requestTypes, out int totalCount)
		{
			var requests = _repository.FindAllRequests(toRequestFilter(input, requestTypes), out totalCount).Where(permissionCheckPredicate).ToList();

			return setupShiftTradeRequestStatus(requests);
		}

		private IEnumerable<IPersonRequest> setupShiftTradeRequestStatus(IEnumerable<IPersonRequest> requests)
		{
			foreach (var request in requests
				.Where (request => request.Request is IShiftTradeRequest)
				.Select (request => request))
			{
				var shiftTradeRequest = (IShiftTradeRequest) request.Request;
				if (request.IsPending || request.IsNew)
				{
					_shiftTradeRequestStatusChecker.Check (shiftTradeRequest);
				}
				else
				{
					_shiftTradeSwapScheduleDetailsMapper.Map(shiftTradeRequest);
				}
				
			}

			return requests;
		}

		private RequestFilter toRequestFilter(AllRequestsFormData input, IEnumerable<RequestType> requestTypes)
		{
			var dateTimePeriod = new DateOnlyPeriod(input.StartDate, input.EndDate).ToDateTimePeriod(_userTimeZone.TimeZone());
			var queryDateTimePeriod = dateTimePeriod.ChangeEndTime(TimeSpan.FromSeconds(-1));

			var filter = new RequestFilter
			{
				RequestFilters = input.Filters,
				Period = queryDateTimePeriod,
				Paging = input.Paging,
				RequestTypes = requestTypes,
				SortingOrders = input.SortingOrders
			};

			if (input.AgentSearchTerm != null)
			{
				filter.Persons = _peopleSearchProvider.SearchPermittedPeople(input.AgentSearchTerm,
					input.StartDate, DefinedRaptorApplicationFunctionPaths.WebRequests);
			}

			return filter;
		}


		private bool permissionCheckPredicate(IPersonRequest request)
		{
			var checkDate = new DateOnly(request.Request.Period.StartDateTime);
			ITeam team = request.Person.MyTeam(checkDate);
			ISite site = team == null ? null : team.Site;
			IBusinessUnit bu = site == null ? null : site.BusinessUnit;


			var authorizeOrganisationDetail = new AuthorizeOrganisationDetail
			{
				PersonId = request.Person.Id ?? Guid.Empty,
				TeamId = team == null? null : team.Id,
				SiteId = site == null? null : site.Id,
				BusinessUnitId = (bu == null || !bu.Id.HasValue) ? Guid.Empty : bu.Id.Value					
			};

			return _permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.WebRequests,
				new DateOnly(request.Request.Period.StartDateTime),
				authorizeOrganisationDetail);
		}

		private class AuthorizeOrganisationDetail : IAuthorizeOrganisationDetail
		{
			public Guid PersonId { get; set; }
			public Guid? TeamId { get; set; }
			public Guid? SiteId { get; set; }
			public Guid BusinessUnitId { get; set; }
		}

	}
}