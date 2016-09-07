using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class FakePeopleSearchProvider : IPeopleSearchProvider
	{	
		private readonly PeopleSummaryModel _model;
		private readonly IList<IPerson> _permittedPeople;
		private readonly IList<IPerson> _peopleWithConfidentialAbsencePermission;
		private readonly IDictionary<DateOnly, IList<IPerson>> _permittedPeopleByDate;
		private bool _enableDateFilter;		

		public FakePeopleSearchProvider(IEnumerable<IPerson> peopleList,IEnumerable<IOptionalColumn> optionalColumns)
		{
			_permittedPeople = new List<IPerson>();
			_peopleWithConfidentialAbsencePermission = new List<IPerson>();
			_permittedPeopleByDate = new Dictionary<DateOnly, IList<IPerson>>();
			_model = new PeopleSummaryModel
			{
				People = peopleList.ToList(),
				OptionalColumns = optionalColumns.ToList()
			};
		}

		public void EnableDateFilter()
		{
			_enableDateFilter = true;
		}

		public PeopleSummaryModel SearchPermittedPeopleSummary(IDictionary<PersonFinderField,string> criteriaDictionary,
			int pageSize,int currentPageIndex,
			DateOnly currentDate,IDictionary<string,bool> sortedColumns,string function)
		{
			return _model;
		}

		public IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField,string> criteriaDictionary,
			DateOnly dateInUserTimeZone,string function)
		{
			if(_enableDateFilter)
			{
				return !_permittedPeopleByDate.ContainsKey(dateInUserTimeZone) ? new List<IPerson>() : _permittedPeopleByDate[dateInUserTimeZone].ToList();
			}
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission
				: _permittedPeople;
		}

		public IEnumerable<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria,
			DateOnly dateInUserTimeZone,string function)
		{
			if(_enableDateFilter)
			{
				return !_permittedPeopleByDate.ContainsKey(dateInUserTimeZone) ? new List<IPerson>() : _permittedPeopleByDate[dateInUserTimeZone].ToList();
			}
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission
				: _permittedPeople;
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(PersonFinderSearchCriteria searchCriteria,DateOnly currentDate,
			string function)
		{

			if(_enableDateFilter)
			{
				return !_permittedPeopleByDate.ContainsKey(currentDate) ? new List<Guid>() : _permittedPeopleByDate[currentDate].Select(p => p.Id.GetValueOrDefault()).ToList();
			}

			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission.Select(p => p.Id.GetValueOrDefault())
				: _permittedPeople.Select(p => p.Id.GetValueOrDefault());
		}
		
		public IEnumerable<IPerson> SearchPermittedPeopleWithAbsence(IEnumerable<IPerson> permittedPeople,
			DateOnly dateInUserTimeZone)
		{
			throw new NotImplementedException();
		}

		public PersonFinderSearchCriteria CreatePersonFinderSearchCriteria(
			IDictionary<PersonFinderField,string> criteriaDictionary,int pageSize,
			int currentPageIndex,DateOnly currentDate,IDictionary<string,bool> sortedColumns)
		{
			return new PersonFinderSearchCriteria(criteriaDictionary,pageSize,currentDate,sortedColumns,currentDate);
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people, DateOnly currentDate, string function)
		{
			return GetPermittedPersonList(people, currentDate, function).Select(p => p.Id.GetValueOrDefault());
		}

		public IEnumerable<IPerson> GetPermittedPersonList(IEnumerable<IPerson> people, DateOnly currentDate, string function)
		{
			if(_enableDateFilter)
			{
				return !_permittedPeopleByDate.ContainsKey(currentDate) ? new List<IPerson>() : _permittedPeopleByDate[currentDate].ToList();
			}
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission
				: _permittedPeople;
		}
	
		public void PopulateSearchCriteriaResult(PersonFinderSearchCriteria search)
		{
			var people = new List<IPerson>();
			var date = search.BelongsToDate;
			if (_enableDateFilter)
			{
				people = !_permittedPeopleByDate.ContainsKey(date) ? new List<IPerson>() : _permittedPeopleByDate[date].ToList();				
			}
			else
			{
				people = _permittedPeople.ToList();
			}

			search.TotalRows = people.Count;
			int row = 0; 
			people.ForEach(p =>
			{
				search.SetRow(row, toPersonFinderDisplayRow(p, date, row + 1));
				row++;
			});
		}

		private PersonFinderDisplayRow toPersonFinderDisplayRow(IPerson p, DateOnly date, int rowNumber)
		{
			var team = p.MyTeam(date);
			var site = team?.Site;
			var bu = site?.BusinessUnit;

			return new PersonFinderDisplayRow
			{
				PersonId = p.Id.GetValueOrDefault(),
				TeamId = team?.Id,
				SiteId = site?.Id,
				BusinessUnitId = bu?.Id ?? Guid.Empty,
				RowNumber = rowNumber
			};
		}

		public void Add(IPerson person)
		{
			_permittedPeople.Add(person);
		}

		public void Add(DateOnly date, IPerson person)
		{
			if (_permittedPeopleByDate.ContainsKey(date))
			{
				_permittedPeopleByDate[date].Add(person);
			}
			else
			{
				_permittedPeopleByDate[date] = new List<IPerson> {person};
			}
		}

		public void AddPersonUnavailableSince(IPerson person,DateOnly date)
		{

		}

		public void AddPersonWithViewConfidentialPermission(IPerson person)
		{
			_peopleWithConfidentialAbsencePermission.Add(person);
		}
		
	}
}
