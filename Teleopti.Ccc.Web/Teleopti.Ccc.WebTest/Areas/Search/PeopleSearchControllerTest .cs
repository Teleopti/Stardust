using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Ccc.Web.Areas.Search.Models;
using Teleopti.Ccc.WebTest.Areas.Global;


namespace Teleopti.Ccc.WebTest.Areas.Search
{
	[GlobalSearchTest]
	public class PeopleSearchControllerTest : IoCTestAttribute
	{
		public PeopleSearchController target;
		public FakePersonFinderReadOnlyRepository PersonFinderRepository;
		public FakePersonRepository PersonRepository;
		public FakeCurrentBusinessUnit CurrentFakeBusinessUnit;
		private ILoggedOnUser loggonUser;
		
		[SetUp]
		public void Setup()
		{
			loggonUser = new FakeLoggedOnUser();
		}

		[Test]
		public void ShouldReturnPeopleSummary()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			var team = TeamFactory.CreateTeam("TestTeam", "TestSite");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today.AddDays(-1),
				team);
			person.WithName(new Name("Ashley", "Andeen"));
			person.Email = "ashley.andeen@abc.com";
			person.SetEmploymentNumber("1011");

			var personId = person.Id.Value;
			var leavingDate = DateOnly.Today.AddDays(1);
			person.TerminatePerson(leavingDate, MockRepository.GenerateMock<IPersonAccountUpdater>());

			var optionalColumn = new OptionalColumn("Cell Phone");
			var optionalColumnValue = new OptionalColumnValue("123456");

			person.SetOptionalColumnValue(optionalColumnValue, optionalColumn);

			target = new PeopleSearchController(new FakePeopleSearchProvider(new[] { person }, new[] { optionalColumn }), loggonUser,
				new FakePersonFinderReadOnlyRepository(), PersonRepository, new FullPermission());

			var result = ((dynamic)target).GetResult("Ashley", 10, 1, "");

			var optionalColumns = (IEnumerable<string>)result.Content.OptionalColumns;
			Assert.AreEqual(1, optionalColumns.Count());
			Assert.AreEqual("Cell Phone", optionalColumns.First());

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			var first = peopleList.First();
			Assert.AreEqual("TestSite/TestTeam", first.Team);
			Assert.AreEqual("Ashley", first.FirstName);
			Assert.AreEqual("Andeen", first.LastName);
			Assert.AreEqual("1011", first.EmploymentNumber);
			Assert.AreEqual(personId, first.PersonId);
			Assert.AreEqual("ashley.andeen@abc.com", first.Email);
			Assert.AreEqual(leavingDate.ToShortDateString(), first.LeavingDate);

			var optionalColumnValues = (IEnumerable<KeyValuePair<string, string>>)first.OptionalColumnValues;
			var columnValue = optionalColumnValues.First();
			Assert.AreEqual("Cell Phone", columnValue.Key);
			Assert.AreEqual("123456", columnValue.Value);
		}

		[Test]
		public void ShouldSortPeopleByLastName()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			var firstPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			var secondPerson = PersonFactory.CreatePersonWithGuid("Abc", "Bac");

			target =
				new PeopleSearchController(
					new FakePeopleSearchProvider(new[] { firstPerson, secondPerson }, new List<IOptionalColumn>()), loggonUser,
					new FakePersonFinderReadOnlyRepository(), PersonRepository, new FullPermission());

			var result = ((dynamic)target).GetResult("a", 10, 1, "");

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			Assert.AreEqual(firstPerson.Name.FirstName, peopleList.First().FirstName);
			Assert.AreEqual(secondPerson.Name.FirstName, peopleList.Last().FirstName);
		}

		[Test]
		public void ShouldReturnMyTeamMembersByDefault()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			var currentUser = loggonUser.CurrentUser();
			currentUser.SetId(Guid.NewGuid());
			currentUser.WithName(new Name("firstName", "lastName"));
			var person = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");

			var team = TeamFactory.CreateTeam("MyTeam", "MySite");
			currentUser.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(-1),
				PersonContractFactory.CreatePersonContract(), team));

			target =
				new PeopleSearchController(new FakePeopleSearchProvider(new[] { person, currentUser }, new List<IOptionalColumn>()),
					loggonUser, new FakePersonFinderReadOnlyRepository(), PersonRepository, new FullPermission());

			var result = ((dynamic)target).GetResult("", 10, 1, "");
			var peopleList = (IEnumerable<dynamic>)result.Content.People;

			Assert.AreEqual(2, peopleList.Count());
			Assert.AreEqual(person.Name.FirstName, peopleList.First().FirstName);
			Assert.AreEqual(currentUser.Name.FirstName, peopleList.Last().FirstName);
		}

		[Test]
		public void ShouldSortPeopleByThreeCriterials()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			var firstPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			firstPerson.SetEmploymentNumber("1");
			var secondPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			secondPerson.SetEmploymentNumber("3");
			var thirdPerson = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			thirdPerson.SetEmploymentNumber("2");

			target =
				new PeopleSearchController(
					new FakePeopleSearchProvider(new[] { firstPerson, secondPerson, thirdPerson }, new List<IOptionalColumn>()),
					loggonUser, new FakePersonFinderReadOnlyRepository(), PersonRepository, new FullPermission());

			var result = ((dynamic)target).GetResult("a", 10, 1, "lastname:true;firstname:true;employmentnumber:true");

			var peopleList = (IEnumerable<dynamic>)result.Content.People;
			var pe = peopleList.ToList();
			Assert.AreEqual(pe[0].EmploymentNumber, "1");
			Assert.AreEqual(pe[1].EmploymentNumber, "2");
			Assert.AreEqual(pe[2].EmploymentNumber, "3");
		}

		[Test]
		public void FindPeople_ShouldReturnPeopleViewModelWithCriteria()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			var p1 = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
			var p2 = PersonFactory.CreatePersonWithGuid("Aston", "Karlsson");
			var p3 = PersonFactory.CreatePersonWithGuid("Kalle", "Anka");
			PersonFinderRepository.Has(p1);
			PersonFinderRepository.Has(p2);
			PersonFinderRepository.Has(p3);
			PersonRepository.Add(p1);
			PersonRepository.Add(p2);
			PersonRepository.Add(p3);

			var inputModel = new FindPeopleInputModel
			{
				KeyWord = "as",
				PageSize = 10,
				PageIndex = 0
			};

			target =
				new PeopleSearchController(
					new FakePeopleSearchProvider(new[] { p1, p2, p3 }, new List<IOptionalColumn>()), loggonUser,
					PersonFinderRepository, PersonRepository, new FullPermission());

			var result = target.FindPeople(inputModel);

			result.People.Count().Should().Be.EqualTo(2);
			result.People.SingleOrDefault(p => p.FirstName.Equals("Ashley")).Should().Not.Be.Null();
			result.People.SingleOrDefault(p => p.FirstName.Equals("Aston")).Should().Not.Be.Null();
		}


		[Test]
		public void FindPeople_ShouldPaginate()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			for (int i = 0; i < 33; i++)
			{
				var ash = PersonFactory.CreatePersonWithGuid($"Ashley {i}", $"Andeen {i}");
				var kal = PersonFactory.CreatePersonWithGuid($"Kalle {i}", $"Anka {i}");
				PersonFinderRepository.Has(ash);
				PersonFinderRepository.Has(kal);

				ash.PermissionInformation.AddApplicationRole(ApplicationRoleFactory.CreateRole("Agent", "Agent").WithId());
				var ashPP = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today).WithId();
				ashPP.Team = TeamFactory.CreateTeam("Prefrences", "London");
				ash.AddPersonPeriod(ashPP);

				kal.PermissionInformation.AddApplicationRole(ApplicationRoleFactory.CreateRole("Agent", "Agent").WithId());
				var kalPP = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today).WithId();
				kalPP.Team = TeamFactory.CreateTeam("Prefrences", "London");
				kal.AddPersonPeriod(kalPP);

				PersonRepository.Add(ash);
				PersonRepository.Add(kal);
			}

			var inputModel = new FindPeopleInputModel
			{
				KeyWord = "as",
				PageSize = 10,
				PageIndex = 0
			};

			target =
				new PeopleSearchController(
					new FakePeopleSearchProvider(new List<IPerson>(), new List<IOptionalColumn>()), loggonUser,
					PersonFinderRepository, PersonRepository, new FullPermission());

			var result = target.FindPeople(inputModel);
			result.People.Count().Should().Be.EqualTo(10);
			result.TotalRows.Should().Be.EqualTo(33);
			result.PageIndex.Should().Be.EqualTo(inputModel.PageIndex);

			inputModel.PageIndex = 3;
			result = target.FindPeople(inputModel);
			result.People.Count().Should().Be.EqualTo(3);
			result.TotalRows.Should().Be.EqualTo(33);
			result.PageIndex.Should().Be.EqualTo(inputModel.PageIndex);

			inputModel.PageIndex = 0;
			inputModel.PageSize = 100;
			result = target.FindPeople(inputModel);
			result.People.Count().Should().Be.EqualTo(33);
			result.TotalRows.Should().Be.EqualTo(33);
			result.PageIndex.Should().Be.EqualTo(inputModel.PageIndex);

			inputModel.PageIndex = 4;
			inputModel.PageSize = 10;
			result = target.FindPeople(inputModel);
			result.People.Count().Should().Be.EqualTo(0);
			result.TotalRows.Should().Be.EqualTo(33);
			result.PageIndex.Should().Be.EqualTo(inputModel.PageIndex);
		}

		[Test]
		public void FindPeople_ShouldMapSearchResultsToViewModel()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));
			var ash = PersonFactory.CreatePersonWithGuid($"Ashley", $"Andeen");
			PersonFinderRepository.Has(ash);
			var appRole = ApplicationRoleFactory.CreateRole("AgentZero", "Agent Zero Description").WithId();
			ash.PermissionInformation.AddApplicationRole(appRole);
			var ashPP = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today).WithId();
			ashPP.Team = TeamFactory.CreateTeam("Prefrences", "London");
			ash.AddPersonPeriod(ashPP);
			PersonRepository.Add(ash);

			var inputModel = new FindPeopleInputModel
			{
				KeyWord = "as",
				PageSize = 10,
				PageIndex = 0
			};

			target =
				new PeopleSearchController(
					new FakePeopleSearchProvider(new[] { ash }, new List<IOptionalColumn>()), loggonUser,
					PersonFinderRepository, PersonRepository, new FullPermission());
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			var result = target.FindPeople(inputModel);
			result.People.Count().Should().Be.EqualTo(1);
			var person = result.People.First();
			person.Id.Should().Be.EqualTo(ash.Id.GetValueOrDefault().ToString());
			person.FirstName.Should().Be.EqualTo(ash.Name.FirstName);
			person.LastName.Should().Be.EqualTo(ash.Name.LastName);
			person.Site.Should().Be.EqualTo(ashPP.Team.Site.Description.Name);
			person.Team.Should().Be.EqualTo(ashPP.Team.Description.Name);
			person.Roles.Count().Should().Be.EqualTo(1);
			var role = person.Roles.First();
			role.Id.Should().Be.EqualTo(appRole.Id);
			role.Name.Should().Be.EqualTo(appRole.DescriptionText);
		}

		[Test]
		public void FindPeople_ShouldSortResultBasedOnFirstName()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			for (int i = 65; i <= 90; i++)
			{
				var asciiChar = Convert.ToChar(i);
				var ash = PersonFactory.CreatePersonWithGuid($"{asciiChar}a", $"Andeen {i}");
				PersonFinderRepository.Has(ash);

				ash.PermissionInformation.AddApplicationRole(ApplicationRoleFactory.CreateRole("Agent", "Agent").WithId());
				var ashPP = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today).WithId();
				ashPP.Team = TeamFactory.CreateTeam("Prefrences", "London");
				ash.AddPersonPeriod(ashPP);

				PersonRepository.Add(ash);
			}

			var inputModel = new FindPeopleInputModel
			{
				KeyWord = "a",
				PageSize = 50,
				PageIndex = 0,
				Direction = 1,
				SortColumn = 0

			};

			target =
				new PeopleSearchController(
					new FakePeopleSearchProvider(new List<IPerson>(), new List<IOptionalColumn>()), loggonUser,
					PersonFinderRepository, PersonRepository, new FullPermission());

			var result = target.FindPeople(inputModel);
			var person = result.People.First();
			person.FirstName.Should().StartWith("A");
		}

		[Test]
		public void FindPeople_ShouldSortDescending()
		{
			CurrentFakeBusinessUnit.OnThisThreadUse(new BusinessUnit("Sweden").WithId(Guid.NewGuid()));

			for (int i = 65; i <= 90; i++)
			{
				var asciiChar = Convert.ToChar(i);
				var ash = PersonFactory.CreatePersonWithGuid($"{asciiChar}a", $"Andeen {i}");
				PersonFinderRepository.Has(ash);

				ash.PermissionInformation.AddApplicationRole(ApplicationRoleFactory.CreateRole("Agent", "Agent").WithId());
				var ashPP = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today).WithId();
				ashPP.Team = TeamFactory.CreateTeam("Prefrences", "London");
				ash.AddPersonPeriod(ashPP);

				PersonRepository.Add(ash);
			}

			var inputModel = new FindPeopleInputModel
			{
				KeyWord = "a",
				PageSize = 50,
				PageIndex = 0,
				Direction = 0,
				SortColumn = 0

			};

			target =
				new PeopleSearchController(
					new FakePeopleSearchProvider(new List<IPerson>(), new List<IOptionalColumn>()), loggonUser,
					PersonFinderRepository, PersonRepository, new FullPermission());


			var result = target.FindPeople(inputModel);
			var person = result.People.First();
			person.FirstName.Should().StartWith("Z");
		}
	}
}
