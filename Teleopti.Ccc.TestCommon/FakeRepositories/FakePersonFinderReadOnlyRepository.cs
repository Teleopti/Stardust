using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonFinderReadOnlyRepository : IPersonFinderReadOnlyRepository
	{
		private readonly IList<IPerson> _personList = new List<IPerson>();
		private readonly IList<PersonExternalLogonInfo> _externalLogonInfos = new List<PersonExternalLogonInfo>();

		public void Has(IPerson person)
		{
			_personList.Add(person);
		}
		public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
		{
			throw new NotImplementedException();
		}

		public void FindInTeams(IPersonFinderSearchCriteria personFinderSearchCriteria, Guid[] teamIds)
		{
			throw new NotImplementedException();
		}

		public void FindPeople(IPeoplePersonFinderSearchCriteria personFinderSearchCriteria)
		{
			var personList = _personList.Where(x =>
				x.Name.FirstName.ToLower().Contains(personFinderSearchCriteria.SearchValue.ToLower()));


			var paginatedResult = personList.Skip((personFinderSearchCriteria.CurrentPage - 1) * personFinderSearchCriteria.PageSize).Take(personFinderSearchCriteria.PageSize);

			var personFinderDisplayRows = new List<PersonFinderDisplayRow>();

			foreach (var personFinderDisplayRow in paginatedResult.Select(p => new PersonFinderDisplayRow { PersonId = p.Id.GetValueOrDefault(), FirstName = p.Name.FirstName, LastName = p.Name.LastName }))
			{
				personFinderDisplayRows.Add(personFinderDisplayRow);
			}

			personFinderSearchCriteria.SetRows(personFinderDisplayRows);

			personFinderSearchCriteria.TotalRows = personList.Count();
		}

		public void FindPeopleWithDataPermission(IPeoplePersonFinderSearchWithPermissionCriteria personFinderSearchCriteria)
		{
			var personList = _personList.Where(x =>
				x.Name.FirstName.ToLower().Contains(personFinderSearchCriteria.SearchValue.ToLower()));


			var paginatedResult = personList.Skip((personFinderSearchCriteria.CurrentPage - 1) * personFinderSearchCriteria.PageSize).Take(personFinderSearchCriteria.PageSize);

			var personFinderDisplayRows = new List<PersonFinderDisplayRow>();

			foreach (var personFinderDisplayRow in paginatedResult.Select(p => new PersonFinderDisplayRow { PersonId = p.Id.GetValueOrDefault(), FirstName = p.Name.FirstName, LastName = p.Name.LastName }))
			{
				personFinderDisplayRows.Add(personFinderDisplayRow);
			}

			IOrderedEnumerable<PersonFinderDisplayRow> tempList;

			switch (personFinderSearchCriteria.SortColumn)
			{
				case 0:
					tempList = personFinderSearchCriteria.SortDirection == 1
						? personFinderDisplayRows.OrderBy(p => p.FirstName)
						: personFinderDisplayRows.OrderByDescending(p => p.FirstName);
					break;
				case 1:
					tempList = personFinderSearchCriteria.SortDirection == 1
						? personFinderDisplayRows.OrderBy(p => p.FirstName)
						: personFinderDisplayRows.OrderByDescending(p => p.FirstName);
					break;
				case 4:
					tempList = personFinderSearchCriteria.SortDirection == 1
						? personFinderDisplayRows.OrderBy(p => p.SiteName).ThenBy(p => p.TeamName)
						: personFinderDisplayRows.OrderByDescending(p => p.SiteName).ThenByDescending(p => p.TeamName);
					break;
				default:
					tempList = personFinderSearchCriteria.SortDirection == 1
						? personFinderDisplayRows.OrderBy(p => p.LastName)
						: personFinderDisplayRows.OrderByDescending(p => p.LastName);
					break;
			}

			personFinderDisplayRows = tempList.ToList();

			personFinderSearchCriteria.SetRows(personFinderDisplayRows);

			personFinderSearchCriteria.TotalRows = personList.Count();
		}

		public void UpdateFindPerson(ICollection<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public void UpdateFindPersonData(ICollection<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public List<Guid> FindPersonIdsInTeams(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			var personList = _personList.Where(a =>
				teamIds!=null && teamIds.Length > 0 && a.MyTeam(date)!=null? teamIds.ToList().Contains(a.MyTeam(date).Id.Value) : true);
			return personList.Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public List<Guid> FindPersonIdsInTeamsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _personList.Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public List<Guid> FindPersonIdsInGroupsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] groupIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _personList.Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public List<Guid> FindPersonIdsInDynamicOptionalGroupPages(DateOnlyPeriod period, Guid groupPageId, string[] dynamicValues,
			IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _personList.Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public void SetPersonExternalLogonInfo(PersonExternalLogonInfo info)
		{
			_externalLogonInfos.Add(info);
		}

		public IList<PersonIdentityMatchResult> FindPersonByIdentities(IEnumerable<string> identities)
		{
			var identityList = identities.ToList();

			var result = new List<PersonIdentityMatchResult>();
			result = _personList.Where(p => identityList.Contains(p.EmploymentNumber)).Select(x =>
				new PersonIdentityMatchResult
				{
					LogonName = x.EmploymentNumber,
					PersonId = x.Id.Value,
					MatchField = IdentityMatchField.EmploymentNumber
				}).ToList();

			result.AddRange(from externalLogon in _externalLogonInfos
							from acdLogonName in externalLogon.ExternalLogonName
							where identityList.Contains(acdLogonName)
							select new PersonIdentityMatchResult
							{
								LogonName = acdLogonName,
								PersonId = externalLogon.PersonId,
								MatchField = IdentityMatchField.ExternalLogon
							});

			return result;
		}

		public bool ValidatePersonIds(List<Guid> ids, DateOnly date, Guid userId, string appFuncForeginId)
		{
			return true;
		}
	}
}