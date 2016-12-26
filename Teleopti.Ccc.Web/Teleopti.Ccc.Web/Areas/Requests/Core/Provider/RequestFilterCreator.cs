using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
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
		RequestFilter Create (AllRequestsFormData input, IEnumerable<RequestType> requestTypes);
	}

	public class RequestFilterCreator : IRequestFilterCreator
	{
		private readonly IPeopleSearchProvider _peopleSearchProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		private readonly IToggleManager _toggleManager;
		private readonly IPersonRepository _personRepository;


		public RequestFilterCreator(IPeopleSearchProvider peopleSearchProvider, IUserTimeZone userTimeZone, IApplicationRoleRepository applicationRoleRepository, IToggleManager toggleManager, IPersonRepository personRepository)
		{
			_peopleSearchProvider = peopleSearchProvider;
			_userTimeZone = userTimeZone;
			_applicationRoleRepository = applicationRoleRepository;
			_toggleManager = toggleManager;
			_personRepository = personRepository;
		}

		public RequestFilter Create(AllRequestsFormData input, IEnumerable<RequestType> requestTypes)
		{
			var dateTimePeriod = new DateOnlyPeriod(input.StartDate, input.EndDate).ToDateTimePeriod(_userTimeZone.TimeZone());
			var queryDateTimePeriod = dateTimePeriod.ChangeEndTime(TimeSpan.FromSeconds(-1));
			var businessHierachyToggle = _toggleManager.IsEnabled(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);

			var filter = new RequestFilter
			{
				RequestFilters = input.Filters,
				Period = queryDateTimePeriod,
				Paging = input.Paging,
				RequestTypes = requestTypes,
				SortingOrders = input.SortingOrders
			};

			if (businessHierachyToggle)
			{
				var searchCriteria = _peopleSearchProvider.CreatePersonFinderSearchCriteria(input.AgentSearchTerm, 9999, 1, input.StartDate, null);
				_peopleSearchProvider.PopulateSearchCriteriaResult(searchCriteria, input.SelectedTeamIds);
				var targetIds = searchCriteria.DisplayRows.Where(r => r.RowNumber > 0).Select(r => r.PersonId).ToList();

				if (targetIds.Count == 0)
					filter.Persons = new List<IPerson>();
				else
				{
					var matchedPersons = _personRepository.FindPeople(targetIds);
					filter.Persons = _peopleSearchProvider.GetPermittedPersonList(matchedPersons, input.StartDate, DefinedRaptorApplicationFunctionPaths.WebRequests).ToList();
				}
			}
			else if(input.AgentSearchTerm.Any())
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