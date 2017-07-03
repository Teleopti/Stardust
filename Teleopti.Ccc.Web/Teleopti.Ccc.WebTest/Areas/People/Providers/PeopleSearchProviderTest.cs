using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.People.Providers
{
	[TestFixture]
	public class PeopleSearchProviderTest
	{
		private IPersonFinderReadOnlyRepository searchRepository;
		private IPersonRepository personRepository;
		private PeopleSearchProvider target;
		private IOptionalColumnRepository optionalColumnRepository;
		private IPermissionProvider permissionProvider;
		private FakePersonAbsenceRepository personAbsenceRepository;
		private ILoggedOnUser loggedOnUser;
		private FakeCurrentBusinessUnit currentBusinessUnit;
		private FakeCurrentScenario currentScenario;

		[SetUp]
		public void Setup()
		{
			searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
			personRepository = new FakePersonRepositoryLegacy();
			optionalColumnRepository = MockRepository.GenerateMock<IOptionalColumnRepository>();
			personAbsenceRepository = new FakePersonAbsenceRepositoryLegacy();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			loggedOnUser = new FakeLoggedOnUser();
			currentScenario = new FakeCurrentScenario();

			currentBusinessUnit = new FakeCurrentBusinessUnit();

			currentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.CreateWithId("bu"));

			target = new PeopleSearchProvider(searchRepository, personRepository,
				new FakePermissionProvider(), optionalColumnRepository, personAbsenceRepository, loggedOnUser, currentBusinessUnit, currentScenario);
		}

		[Test]
		public void ShouldSearchForPeople()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today,
				new Team().WithDescription(new Description("TestTeam")));
			person.WithName(new Name("Ashley", "Andeen"));
			person.Email = "ashley.andeen@abc.com";
			person.SetEmploymentNumber("1011");

			var personId = person.Id.Value;
			person.TerminatePerson(new DateOnly(2025, 4, 9), MockRepository.GenerateMock<IPersonAccountUpdater>());
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
			personRepository.Add(person);

			var optionalColumn = new OptionalColumn("CellPhone");
			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>
			{
				optionalColumn
			});

			var optionalColumnValue = new OptionalColumnValue("123456");
			person.AddOptionalColumnValue(optionalColumnValue, optionalColumn);

			var searchCriteria = new Dictionary<PersonFinderField, string>
			{
				{
					PersonFinderField.All, "Ashley"
				}
			};
			var result = target.SearchPermittedPeopleSummary(searchCriteria, 10, 1, DateOnly.Today, new Dictionary<string, bool>(), DefinedRaptorApplicationFunctionPaths.WebPeople);

			var peopleList = result.People;
			var optionalColumns = result.OptionalColumns;
			optionalColumns.Count.Should().Be.EqualTo(1);
			optionalColumns.First().Name.Should().Be.EqualTo("CellPhone");

			var first = peopleList.First();
			first.Name.FirstName.Should().Be.EqualTo("Ashley");
			first.Name.LastName.Should().Be.EqualTo("Andeen");
			first.EmploymentNumber.Should().Be.EqualTo("1011");
			first.Id.Should().Be.EqualTo(personId);
			first.Email.Should().Be.EqualTo("ashley.andeen@abc.com");
			first.MyTeam(DateOnly.Today).Description.Name.Should().Be.EqualTo("TestTeam");
		}

		[Test]
		public void ShouldSearchForPeopleWithNoPermission()
		{
			var personId = Guid.NewGuid();
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
			permissionProvider.Stub(
				x =>
					x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.WebPeople, DateOnly.Today,
						personFinderDisplayRow)).Return(false);
			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>());

			var searchCriteria = new Dictionary<PersonFinderField, string>
			{
				{
					PersonFinderField.All, "Ashley"
				}
			};
			var result = target.SearchPermittedPeopleSummary(searchCriteria, 10, 1, DateOnly.Today, new Dictionary<string, bool>(), DefinedRaptorApplicationFunctionPaths.WebPeople);
			var peopleList = result.People;
			peopleList.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSearchPermittedPeopleWithAbsence()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today,
				new Team().WithDescription(new Description("TestTeam")));
			person.WithName(new Name("John", "Smith"));
			person.Email = "john.smith@abc.com";
			person.SetEmploymentNumber("1012");

			var personId = person.Id.Value;
			var personFinderDisplayRow = new PersonFinderDisplayRow
			{
				FirstName = "John",
				LastName = "Smith",
				EmploymentNumber = "1012",
				PersonId = personId,
				RowNumber = 1
			};

			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				c.SetRow(1, personFinderDisplayRow);
				return true;
			}));
			personRepository.Add(person);

			personAbsenceRepository.Add(createPersonAbsence(person));
			target = new PeopleSearchProvider(searchRepository, personRepository, new FakePermissionProvider(), optionalColumnRepository, personAbsenceRepository,
				loggedOnUser, currentBusinessUnit, currentScenario);

			var searchCriteria = new Dictionary<PersonFinderField, string>
			{
				{
					PersonFinderField.All, "John"
				}
			};

			var permittedPeople = target.SearchPermittedPeople(searchCriteria, new DateOnly(2016, 3, 1),
				DefinedRaptorApplicationFunctionPaths.WebPeople);
			var result = target.SearchPermittedPeopleWithAbsence(permittedPeople, new DateOnly(2016, 3, 1));
			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSearchPermittedPeopleReturnMoreThan2Pages()
		{
			personRepository = new FakePersonRepositoryLegacy();
			target = new PeopleSearchProvider(searchRepository, personRepository,
				new FakePermissionProvider(), optionalColumnRepository, personAbsenceRepository, loggedOnUser, currentBusinessUnit, currentScenario);

			var searchCriteria = new Dictionary<PersonFinderField, string>
			{
				{
					PersonFinderField.Role, "Agent"
				}
			};

			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				c.TotalRows = 500;
				for (var i = 1; i <= c.TotalRows; i++)
				{
					var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.Today);
					person.SetId(Guid.NewGuid());
					person.WithName(new Name(string.Format("Agent{0:000}", i), string.Format("Andeen{0:000}", i)));
					person.SetEmploymentNumber(i.ToString("0000"));
					personRepository.Add(person);

					var personFinderDisplayRow = new PersonFinderDisplayRow
					{
						FirstName = person.Name.FirstName,
						LastName = person.Name.LastName,
						EmploymentNumber = person.EmploymentNumber,
						PersonId = person.Id.GetValueOrDefault(),
						RowNumber = i
					};

					c.SetRow(i, personFinderDisplayRow);

					permissionProvider.Stub(
						x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.WebPeople, DateOnly.Today,
							personFinderDisplayRow)).Return(true);
				}
				return true;
			}));


			var result = target.SearchPermittedPeopleSummary(searchCriteria, 20, 1, DateOnly.Today, new Dictionary<string, bool>(), DefinedRaptorApplicationFunctionPaths.WebPeople);
			result.TotalPages.Should().Be.GreaterThan(2);
		}

		private IPersonAbsence createPersonAbsence(IPerson person)
		{
			return PersonAbsenceFactory.CreatePersonAbsence(person, currentScenario.Current(), new DateTimePeriod(2016, 3, 1, 7, 2016, 3, 1, 15));
		}
	}
}
