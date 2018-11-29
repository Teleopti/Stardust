using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


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
		private FakeCurrentBusinessUnit currentBusinessUnit;
		private FakeLoggedOnUser fakeLoggedOnUser;

		[SetUp]
		public void Setup()
		{
			searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
			personRepository = new FakePersonRepositoryLegacy();
			optionalColumnRepository = MockRepository.GenerateMock<IOptionalColumnRepository>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			currentBusinessUnit = new FakeCurrentBusinessUnit();
			currentBusinessUnit.FakeBusinessUnit(BusinessUnitFactory.CreateWithId("bu"));
			fakeLoggedOnUser = new FakeLoggedOnUser();
			target = new PeopleSearchProvider(searchRepository, personRepository,
				new FakePermissionProvider(), optionalColumnRepository, currentBusinessUnit, fakeLoggedOnUser);
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
			person.SetOptionalColumnValue(optionalColumnValue, optionalColumn);

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
		public void ShouldSearchPermittedPeopleReturnMoreThan2Pages()
		{
			personRepository = new FakePersonRepositoryLegacy();
			target = new PeopleSearchProvider(searchRepository, personRepository,
				new FakePermissionProvider(), optionalColumnRepository, currentBusinessUnit, fakeLoggedOnUser);

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
					person.WithName(new Name($"Agent{i:000}", $"Andeen{i:000}"));
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
	}
}
