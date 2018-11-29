using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;


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
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly IPermissionProvider _permissionProvider;


		public RequestFilterCreator(IPeopleSearchProvider peopleSearchProvider, IUserTimeZone userTimeZone, IApplicationRoleRepository applicationRoleRepository, IGroupingReadOnlyRepository groupingReadOnlyRepository, IPermissionProvider permissionProvider)
		{
			_peopleSearchProvider = peopleSearchProvider;
			_userTimeZone = userTimeZone;
			_applicationRoleRepository = applicationRoleRepository;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_permissionProvider = permissionProvider;
		}

		public RequestFilter Create(AllRequestsFormData input, IEnumerable<RequestType> requestTypes)
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

			adjustRoleFieldValue(input.AgentSearchTerm);

			List<Guid> targetIds;

			var period = new DateOnlyPeriod(input.StartDate, input.EndDate);
			targetIds = !input.IsDynamic ? _peopleSearchProvider.FindPersonIdsInPeriodWithGroup(period, input.GroupIds, input.AgentSearchTerm)
										: _peopleSearchProvider.FindPersonIdsInPeriodWithDynamicGroup(period, input.SelectedGroupPageId, input.DynamicOptionalValues, input.AgentSearchTerm);

			if (targetIds.Count == 0)
				filter.Persons = new List<IPerson>();
			else
			{
				var matchedPersons = _groupingReadOnlyRepository.DetailsForPeople(targetIds);
				filter.Persons = matchedPersons.Where(p => _permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.WebRequests, input.StartDate, p)).Select(
					p =>
					{
						var person = new Person();
						person.SetId(p.PersonId);
						return person;
					}).ToList();
			}

			return filter;
		}

		private void adjustRoleFieldValue(IDictionary<PersonFinderField, string> agentSearchTerm)
		{
			if (!agentSearchTerm.Any() || !agentSearchTerm.ContainsKey(PersonFinderField.Role))
				return;

			var roleNameValues = agentSearchTerm[PersonFinderField.Role];
			if (string.IsNullOrWhiteSpace(roleNameValues))
				return;

			var separator = ";";
			var roleNames = roleNameValues.Split(separator[0]);
			var adjustedRoleNames = new List<string>(roleNames.Length);
			foreach (var roleName in roleNames)
			{
				if (string.IsNullOrWhiteSpace(roleName))
					continue;

				adjustedRoleNames.Add(_applicationRoleRepository.ExistsRoleWithDescription(roleName) ? $"\"{roleName}\"" : roleName);
			}

			agentSearchTerm[PersonFinderField.Role] = string.Join(separator, adjustedRoleNames);
		}

	}
}