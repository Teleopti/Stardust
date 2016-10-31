using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestFilterCreator
	{
		RequestFilter Create (AllRequestsFormData input, IEnumerable<RequestType> requestTypes);
	}

	public class RequestFilterCreator : IRequestFilterCreator
	{
		private readonly IPeopleSearchProvider _peopleSearchProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IApplicationRoleRepository _applicationRoleRepository;

		public RequestFilterCreator(IPeopleSearchProvider peopleSearchProvider, IUserTimeZone userTimeZone, IApplicationRoleRepository applicationRoleRepository)
		{
			_peopleSearchProvider = peopleSearchProvider;
			_userTimeZone = userTimeZone;
			_applicationRoleRepository = applicationRoleRepository;
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

			if (input.AgentSearchTerm != null)
			{
				adjustRoleFieldValue(input.AgentSearchTerm);
				filter.Persons = _peopleSearchProvider.SearchPermittedPeople(input.AgentSearchTerm,
					input.StartDate, DefinedRaptorApplicationFunctionPaths.WebRequests);
			}

			return filter;
		}

		private void adjustRoleFieldValue(IDictionary<PersonFinderField, string> agentSearchTerm)
		{
			if (!agentSearchTerm.ContainsKey(PersonFinderField.Role))
				return;

			var roleNameValues = agentSearchTerm[PersonFinderField.Role];
			if (string.IsNullOrWhiteSpace(roleNameValues))
				return;

			var separator = ";";
			var roleNames = roleNameValues.Split(separator[0]);
			var adjustedRoleNames = new List<string>(roleNames.Length);
			foreach (var roleName  in roleNames)
			{
				if (string.IsNullOrWhiteSpace(roleName))
					continue;

				adjustedRoleNames.Add(_applicationRoleRepository.ExistsRoleWithDescription(roleName) ? $"\"{roleName}\"" : roleName);
			}

			agentSearchTerm[PersonFinderField.Role] = string.Join(separator, adjustedRoleNames);
		}

	}
}