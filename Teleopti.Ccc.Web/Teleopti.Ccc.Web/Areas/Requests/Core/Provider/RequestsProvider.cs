using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class RequestsProvider : IRequestsProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPeopleSearchProvider _peopleSearchProvider;

		public RequestsProvider(IPersonRequestRepository repository, IUserTimeZone userTimeZone, IPermissionProvider permissionProvider, IPeopleSearchProvider peopleSearchProvider)
		{
			_repository = repository;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
			_peopleSearchProvider = peopleSearchProvider;
		}

		public IEnumerable<IPersonRequest> RetrieveRequests(AllRequestsFormData input, out int totalCount)
		{
			return _repository.FindAllRequests(toRequestFilter(input), out totalCount).Where(permissionCheckPredicate);
		}

		private RequestFilter toRequestFilter(AllRequestsFormData input)
		{
			var filter = new RequestFilter
			{
				Period = new DateOnlyPeriod(input.StartDate, input.EndDate).ToDateTimePeriod(_userTimeZone.TimeZone()),
				Paging = input.Paging,
				RequestTypes = new List<RequestType> { RequestType.AbsenceRequest, RequestType.TextRequest },
				SortingOrders = input.SortingOrders
			};

			if (!String.IsNullOrEmpty(input.AgentSearchTerm))
			{
				filter.Persons = _peopleSearchProvider.SearchPermittedPeople(SearchTermParser.Parse(input.AgentSearchTerm),
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