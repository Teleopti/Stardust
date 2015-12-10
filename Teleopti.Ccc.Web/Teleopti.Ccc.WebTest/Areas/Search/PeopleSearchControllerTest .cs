using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	[TestFixture]
	public class PeopleSearchControllerTest
	{
		private PeopleSearchController target;
		private ILoggedOnUser loggonUser;

		[SetUp]
		public void Setup()
		{
			loggonUser = new FakeLoggedOnUser();
		}

		[Test]
		public void ShouldReturnPeopleSummary()
		{
			var team = TeamFactory.CreateTeam("TestTeam", "TestSite");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today.AddDays(-1),
				team);
			person.Name = new Name("Ashley", "Andeen");
			person.Email = "ashley.andeen@abc.com";
			person.EmploymentNumber = "1011";

			var personId = person.Id.Value;
			var leavingDate = DateOnly.Today.AddDays(1);
			person.TerminatePerson(leavingDate, MockRepository.GenerateMock<IPersonAccountUpdater>());

			var optionalColumn = new OptionalColumn("Cell Phone");
			var optionalColumnValue = new OptionalColumnValue("123456");

			person.AddOptionalColumnValue(optionalColumnValue, optionalColumn);

			target = new PeopleSearchController(new FakePeopleSearchProvider(new []{person}, new []{optionalColumn}), loggonUser);

			var result = ((dynamic)target).GetResult("Ashley", 10, 1, "");

			var optionalColumns = (IEnumerable<string>)result.Content.OptionalColumns;
			Assert.AreEqual(1, optionalColumns.Count());
			Assert.AreEqual("Cell Phone", optionalColumns.First());

			var peopleList = (IEnumerable<dynamic>) result.Content.People;
			var first = peopleList.First();
			Assert.AreEqual("TestSite/TestTeam", first.Team);
			Assert.AreEqual("Ashley", first.FirstName);
			Assert.AreEqual("Andeen",first.LastName);
			Assert.AreEqual("1011",first.EmploymentNumber);
			Assert.AreEqual(personId, first.PersonId);
			Assert.AreEqual("ashley.andeen@abc.com",first.Email);
			Assert.AreEqual(leavingDate.ToShortDateString(), first.LeavingDate);

			var optionalColumnValues = (IEnumerable<KeyValuePair<string, string>>)first.OptionalColumnValues;
			var columnValue = optionalColumnValues.First();
			Assert.AreEqual("Cell Phone", columnValue.Key);
			Assert.AreEqual("123456", columnValue.Value);
		}

		[Test]
		public void ShouldSortPeopleByLastName()
		{
			var firstPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			var secondPerson = PersonFactory.CreatePersonWithGuid("Abc", "Bac");

			target = new PeopleSearchController(new FakePeopleSearchProvider(new[] { firstPerson, secondPerson }, new List<IOptionalColumn>()), loggonUser);

			var result = ((dynamic) target).GetResult("a", 10, 1, "");

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			Assert.AreEqual(firstPerson.Name.FirstName, peopleList.First().FirstName);
			Assert.AreEqual(secondPerson.Name.FirstName, peopleList.Last().FirstName);
		}

		[Test]
		public void ShouldReturnMyTeamMembersByDefault()
		{
			var currentUser = loggonUser.CurrentUser();
			currentUser.SetId(Guid.NewGuid());
			currentUser.Name = new Name("firstName", "lastName");
			var person = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");

			var team = TeamFactory.CreateTeam("MyTeam", "MySite");
			currentUser.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(-1),
				PersonContractFactory.CreatePersonContract(), team));

			target = new PeopleSearchController(new FakePeopleSearchProvider(new[] { person, currentUser }, new List<IOptionalColumn>()), loggonUser);

			var result = ((dynamic)target).GetResult("", 10, 1, "");
			var peopleList = (IEnumerable<dynamic>)result.Content.People;

			Assert.AreEqual(2, peopleList.Count());
			Assert.AreEqual(person.Name.FirstName, peopleList.First().FirstName);
			Assert.AreEqual(currentUser.Name.FirstName, peopleList.Last().FirstName);
		}

		[Test]
		public void ShouldSortPeopleByThreeCriterials()
		{
			var firstPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			firstPerson.EmploymentNumber = "1";
			var secondPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			secondPerson.EmploymentNumber = "3";
			var thirdPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			thirdPerson.EmploymentNumber = "2";

			target = new PeopleSearchController(new FakePeopleSearchProvider(new[] { firstPerson, secondPerson, thirdPerson }, new List<IOptionalColumn>()), loggonUser);

			var result = ((dynamic)target).GetResult("a", 10, 1, "lastname:true;firstname:true;employmentnumber:true");

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			var pe = peopleList.ToList();
			Assert.AreEqual(pe[0].EmploymentNumber, "1");
			Assert.AreEqual(pe[1].EmploymentNumber, "2");
			Assert.AreEqual(pe[2].EmploymentNumber, "3");
		}

	}

	public class FakePeopleSearchProvider : IPeopleSearchProvider
	{
		private readonly PeopleSummaryModel _model;
		private IList<IPerson> _permittedPeople;
		private IList<IPerson> _peopleWithConfidentialAbsencePermission; 

		public FakePeopleSearchProvider(IEnumerable<IPerson> peopleList, IEnumerable<IOptionalColumn> optionalColumns  )
		{
			_permittedPeople = new List<IPerson>();
			_peopleWithConfidentialAbsencePermission = new List<IPerson>();
			_model = new PeopleSummaryModel
			{
				People = peopleList.ToList(),
				OptionalColumns = optionalColumns.ToList()
			};
		}

		public PeopleSummaryModel SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize, int currentPageIndex,
			DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			return _model;
		}

		public IEnumerable<IPerson> SearchPermittedPeople(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, string function)
		{
			if (function == DefinedRaptorApplicationFunctionPaths.ViewConfidential)
			{
				return _peopleWithConfidentialAbsencePermission;
			}

			return _permittedPeople;
		}

		public IEnumerable<Guid> GetPermittedPersonIdList(IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize, int currentPageIndex,
			DateOnly currentDate, IDictionary<string, bool> sortedColumns, string function)
		{
			if (function == DefinedRaptorApplicationFunctionPaths.ViewConfidential)
			{
				return _peopleWithConfidentialAbsencePermission.Select(p => p.Id.GetValueOrDefault());
			}
			return _permittedPeople.Select(p => p.Id.GetValueOrDefault());
		}

		public void Add(IPerson person)
		{
			_permittedPeople.Add(person);
		}

		public void AddPersonWithViewConfidentialPermission(IPerson person)
		{
			_peopleWithConfidentialAbsencePermission.Add(person);
		}
	}
}
