using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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
			peopleSearchProvider.Stub(x => x.SearchPeople("Ashley", 50, 1, DateOnly.Today))
				.IgnoreArguments()
				.Return(new PeopleSummaryModel
				{
					People = new List<IPerson>
					{
						person
					},
					OptionalColumns = new List<IOptionalColumn>
					{
						optionalColumn
					}
				});

			var result = ((dynamic) target).GetResult("Ashley", 10, 1);

			var optionalColumns = (IEnumerable<string>)result.Content.OptionalColumns;
			optionalColumns.Count().Equals(1);
			optionalColumns.First().Equals("CellPhone");

			var peopleList = (IEnumerable<dynamic>) result.Content.People;
			var first = peopleList.First();
			first.Team.Equals("TestTeam");
			first.FirstName.Equals("Ashley");
			first.LastName.Equals("Andeen");
			first.EmploymentNumber.Equals("1011");
			first.PersonId.Equals(personId);
			first.Email.Equals("ashley.andeen@abc.com");
			first.LeavingDate.Equals("2015-04-09");

			var optionalColumnValues = (IEnumerable<KeyValuePair<string, string>>)first.OptionalColumnValues;
			var columnValue = optionalColumnValues.First();
			columnValue.Key.Equals("CellPhone");
			columnValue.Value.Equals("123456");
		}

		[Test]
		public void ShouldSortPeopleByLastName()
		{
			var firstPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			var secondPerson = PersonFactory.CreatePersonWithGuid("Abc", "Bac");

			peopleSearchProvider.Stub(x => x.SearchPeople("Ashley", 50, 1, DateOnly.Today))
				.IgnoreArguments()
				.Return(new PeopleSummaryModel
				{
					People = new List<IPerson>
					{
						firstPerson,
						secondPerson
					},
					OptionalColumns = new List<IOptionalColumn>()
				});

			var result = ((dynamic) target).GetResult("a", 10, 1);

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			peopleList.First().FirstName.Equals(firstPerson.Name.FirstName);
			peopleList.Last().FirstName.Equals(secondPerson.Name.FirstName);
		}
	}
}
