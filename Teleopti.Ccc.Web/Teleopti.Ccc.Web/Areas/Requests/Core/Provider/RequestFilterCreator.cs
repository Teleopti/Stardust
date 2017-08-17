using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestFilterCreator
	{
		RequestFilter Create(AllRequestsFormData input, IEnumerable<RequestType> requestTypes);
	}

	public class RequestFilterCreator : IRequestFilterCreator
	{
		private readonly IPeopleSearchProvider _peopleSearchProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IToggleManager _toggleManager;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IPermissionProvider _permissionProvider;


		public RequestFilterCreator(IPeopleSearchProvider peopleSearchProvider, IUserTimeZone userTimeZone,
			IToggleManager toggleManager, IGroupingReadOnlyRepository groupingReadOnlyRepository,
			IPermissionProvider permissionProvider)
		{
			_peopleSearchProvider = peopleSearchProvider;
			_userTimeZone = userTimeZone;
			_toggleManager = toggleManager;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_permissionProvider = permissionProvider;
		}

		public RequestFilter Create(AllRequestsFormData input, IEnumerable<RequestType> requestTypes)
		{
			var period = new DateOnlyPeriod(input.StartDate, input.EndDate);
			var dateTimePeriod = period.ToDateTimePeriod(_userTimeZone.TimeZone());
			var queryDateTimePeriod = dateTimePeriod.ChangeEndTime(TimeSpan.FromSeconds(-1));
			var filter = new RequestFilter
			{
				RequestFilters = input.Filters,
				Period = queryDateTimePeriod,
				Paging = input.Paging,
				RequestTypes = requestTypes,
				SortingOrders = input.SortingOrders
			};

			List<Guid> targetIds;
			if (_toggleManager.IsEnabled(Toggles.Wfm_GroupPages_45057))
			{
				targetIds = _peopleSearchProvider.FindPersonIdsInPeriodWithGroup(period, input.SelectedGroupIds, 
					input.AgentSearchTerm);
			}
			else if (_toggleManager.IsEnabled(Toggles.Wfm_SearchAgentBasedOnCorrectPeriod_44552))
			{
				targetIds = _peopleSearchProvider.FindPersonIdsInPeriod(period, input.SelectedGroupIds, input.AgentSearchTerm);
			}
			else
			{
				targetIds = _peopleSearchProvider.FindPersonIds(input.StartDate, input.SelectedGroupIds, input.AgentSearchTerm);
			}

			if (targetIds.Count == 0)
				filter.Persons = new List<IPerson>();
			else
			{
				const string functionPath = DefinedRaptorApplicationFunctionPaths.WebRequests;
				var matchedPersons = _groupingReadOnlyRepository.DetailsForPeople(targetIds);
				filter.Persons = matchedPersons
					.Where(p => _permissionProvider.HasOrganisationDetailPermission(functionPath, input.StartDate, p))
					.Select(p =>
					{
						var person = new Person();
						person.SetId(p.PersonId);
						return person;
					}).ToList();
			}

			return filter;
		}
	}
}
