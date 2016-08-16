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

		public FakePeopleSearchProvider(IEnumerable<IPerson> peopleList,IEnumerable<IOptionalColumn> optionalColumns)
		{
			_permittedPeople = new List<IPerson>();
			_peopleWithConfidentialAbsencePermission = new List<IPerson>();
			_model = new PeopleSummaryModel
			{
				People = peopleList.ToList(),
				OptionalColumns = optionalColumns.ToList()
			};
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
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission
				: _permittedPeople;
		}

		public IEnumerable<IPerson> SearchPermittedPeople(PersonFinderSearchCriteria searchCriteria,
			DateOnly dateInUserTimeZone,string function)
		{
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission
				: _permittedPeople;
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(PersonFinderSearchCriteria searchCriteria,DateOnly currentDate,
			string function)
		{
			return function == DefinedRaptorApplicationFunctionPaths.ViewConfidential
				? _peopleWithConfidentialAbsencePermission.Select(p => p.Id.GetValueOrDefault())
				: _permittedPeople.Select(p => p.Id.GetValueOrDefault());
		}

		public IEnumerable<Guid> GetPermittedPersonIdListInWeek(PersonFinderSearchCriteria searchCriteria,DateOnly currentDate,
			string function)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(currentDate,DayOfWeek.Monday);
			var week = new DateOnlyPeriod(firstDayOfWeek,firstDayOfWeek.AddDays(6));

			return
				week.DayCollection().SelectMany(d => GetPermittedPersonIdList(searchCriteria,d,function)).Distinct().ToList();
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

		public IEnumerable<Guid> GetPermittedPersonIdList(IEnumerable<IPerson> people,DateOnly currentDate,string function)
		{
			return _permittedPeople.Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public void Add(IPerson person)
		{
			_permittedPeople.Add(person);
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
