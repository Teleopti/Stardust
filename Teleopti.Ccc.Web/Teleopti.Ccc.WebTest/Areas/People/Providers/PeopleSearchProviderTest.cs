using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WebTest.Areas.People.Providers
{
	[TestFixture]
	class PeopleSearchProviderTest
	{
		private IPersonFinderReadOnlyRepository searchRepository;
		private IPersonRepository personRepository;
		private PeopleSearchProvider target;
		private IOptionalColumnRepository optionalColumnRepository;
		private ILoggedOnUser loggedOnUser;

		[SetUp]
		public void Setup()
		{
			searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			optionalColumnRepository = MockRepository.GenerateMock<IOptionalColumnRepository>();
			loggedOnUser = new FakeLoggedOnUser();
			target = new PeopleSearchProvider(searchRepository, personRepository, new FakePermissionProvider(), optionalColumnRepository, loggedOnUser);
		}

		[Test]
		public void ShouldSearchForPeople()
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
			var personFinderDisplayRow = new PersonFinderDisplayRow
			{
				FirstName = "Ashley",
				LastName = "Andeen",
				EmploymentNumber = "1011",
				PersonId = personId,
				RowNumber = 1
			};

			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				c.SetRow(1, personFinderDisplayRow);
				return true;
			}));
			personRepository.Stub(x => x.FindPeople(new List<Guid> { personId })).IgnoreArguments().Return(new List<IPerson> { person });
			var optionalColumn = new OptionalColumn("CellPhone");
			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>()
            {
                optionalColumn
            });
			var optionalColumnValue = new OptionalColumnValue("123456");
			person.AddOptionalColumnValue(optionalColumnValue, optionalColumn);

			var result = target.SearchPeople("Ashley", 10, 1, DateOnly.Today);

			var peopleList = result.People;
			var optionalColumns = result.OptionalColumns;
			optionalColumns.Count().Equals(1);
			optionalColumns.First().Equals("CellPhone");
			var first = peopleList.First();
			first.Name.FirstName.Equals("Ashley");
			first.Name.LastName.Equals("Andeen");
			first.EmploymentNumber.Equals("1011");
			first.Id.Equals(personId);
			first.Email.Equals("ashley.andeen@abc.com");
			//first.LeavingDate.Equals("2015-04-09");
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
			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				c.SetRow(1, personFinderDisplayRow);
				c.SetRow(2, personFinderDisplayRow2);
				return true;
			}));
			personRepository.Stub(x => x.FindPeople(new[] { person.Id.GetValueOrDefault(), personSec.Id.GetValueOrDefault() })).IgnoreArguments().Return(new List<IPerson> { person });
			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>());

			var result = target.SearchPeople("a", 10, 1, DateOnly.Today);

			var peopleList = result.People;
			peopleList.First().Name.FirstName.Equals("Ashley");
			peopleList.Last().Name.FirstName.Equals("Abc");
		}

		[Test]
		public void ShouldSearchForPeopleWithNoPermission()
		{
			var personId = Guid.NewGuid();
			var personFinderDisplayRow = new PersonFinderDisplayRow { FirstName = "Ashley", LastName = "Andeen", EmploymentNumber = "1011", PersonId = personId, RowNumber = 1 };

			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				c.SetRow(1, personFinderDisplayRow);
				return true;
			}));
			personRepository.Stub(x => x.FindPeople(new List<Guid>())).IgnoreArguments().Return(new List<IPerson>());
			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>());

			var result = target.SearchPeople("Ashley", 10, 1, DateOnly.Today);
			var peopleList = result.People;
			peopleList.Count().Equals(0);
		}

		[Test]
		public void ShouldReturnMyTeamMembersByDefault()
		{
			var currentUser = loggedOnUser.CurrentUser();
			currentUser.SetId(new Guid());
			currentUser.Name = new Name("firstName", "lastName");
			var person = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");

			var team = TeamFactory.CreateTeam("MyTeam", "MySite");
			currentUser.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(-1),
				PersonContractFactory.CreatePersonContract(), team));

			var personFinderDisplayRow = new PersonFinderDisplayRow
			{
				FirstName = currentUser.Name.FirstName,
				LastName = currentUser.Name.LastName,
				EmploymentNumber = "1011",
				PersonId = currentUser.Id.Value,
				RowNumber = 1
			};

			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				c.SetRow(1, personFinderDisplayRow);
				return true;
			}));
			personRepository.Stub(x => x.FindPeople(new List<Guid>()))
				.IgnoreArguments()
				.Return(new List<IPerson> { currentUser });
			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>());

			var result = target.SearchPeople("", 10, 1, DateOnly.Today);
			var peopleList = result.People;
			peopleList.Count().Equals(1);
			peopleList.First().Name.FirstName.Equals(currentUser.Name.FirstName);
			peopleList.Where(x => x.Name.FirstName == person.Name.FirstName).Count().Equals(0);
		}
	}
}
