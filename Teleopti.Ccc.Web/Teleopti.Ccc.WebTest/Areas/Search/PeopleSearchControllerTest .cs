using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	[TestFixture]
	public class PeopleSearchControllerTest
	{
		private PeopleSearchController target;
		private IPeopleSearchProvider peopleSearchProvider;

		[SetUp]
		public void Setup()
		{
			peopleSearchProvider = MockRepository.GenerateMock<IPeopleSearchProvider>();
			target = new PeopleSearchController(peopleSearchProvider);
		}

		[Test]
		public void ShouldReturnPeopleSummary()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today,
				new Team
				{
					Description = new Description("TestTeam")
				});
			person.Name = new Name("Ashley", "Andeen");
			person.Email = "ashley.andeen@abc.com";
			var personId = person.Id.Value;
			person.TerminatePerson(new DateOnly(2015, 4, 9), MockRepository.GenerateMock<IPersonAccountUpdater>());

			var optionalColumn = new OptionalColumn("Cell Phone");
			var optionalColumnValue = new OptionalColumnValue("123456");

			person.AddOptionalColumnValue(optionalColumnValue, optionalColumn);
			peopleSearchProvider.Stub(x => x.SearchPeople("Ashley", 50, 1, DateOnly.Today)).IgnoreArguments().Return(new PeopleSummaryModel
			{
				People = new List<IPerson>() { person },
				OptionalColumns = new List<IOptionalColumn>() { optionalColumn }
			});
			
			var result = ((dynamic)target).GetResult("Ashley", 10, 1);

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			var optionalColumns = (IEnumerable<string>)result.Content.OptionalColumns;
			optionalColumns.Count().Equals(1);
			optionalColumns.First().Equals("CellPhone");
			var first = peopleList.First();
			first.Team.Equals("TestTeam");
			first.FirstName.Equals("Ashley");
			first.LastName.Equals("Andeen");
			first.EmploymentNumber.Equals("1011");
			first.PersonId.Equals(personId);
			first.Email.Equals("ashley.andeen@abc.com");
			//first.LeavingDate.Equals("2015-04-09");
			var optionalColumnValues = (IEnumerable<KeyValuePair<string, string>>)first.OptionalColumnValues;
			optionalColumnValues.First().Key.Equals("CellPhone");
			optionalColumnValues.First().Value.Equals("123456");
		}

		[Test]
		public void ShouldSortPeopleByLastName()
		{
			var person = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			var personSec = PersonFactory.CreatePersonWithGuid("Abc", "Bac");

			var personFinderDisplayRow = new PersonFinderDisplayRow
			{
				FirstName = "Ashley",
				LastName = "Andeen",
				EmploymentNumber = "1011",
				PersonId = person.Id.GetValueOrDefault(),
				RowNumber = 1
			};

			var personFinderDisplayRow2 = new PersonFinderDisplayRow
			{
				FirstName = "Abc",
				LastName = "Bac",
				EmploymentNumber = "1012",
				PersonId = personSec.Id.GetValueOrDefault(),
				RowNumber = 2
			};

			peopleSearchProvider.Stub(x => x.SearchPeople("Ashley", 50, 1, DateOnly.Today)).IgnoreArguments().Return(new PeopleSummaryModel
			{
				People = new List<IPerson>() { person, personSec },
				OptionalColumns = new List<IOptionalColumn>() { new OptionalColumn("Cell Phone")}
			});

			var result = ((dynamic)target).GetResult("a", 10, 1);

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			peopleList.First().FirstName.Equals("Ashley");
			peopleList.Last().FirstName.Equals("Abc");
		}
	}
}

