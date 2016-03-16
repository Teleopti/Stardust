using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
		private IPersonAbsenceRepository personAbsenceRepository;
		private ILoggedOnUser loggedOnUser;

		[SetUp]
		public void Setup()
		{
			searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			optionalColumnRepository = MockRepository.GenerateMock<IOptionalColumnRepository>();
			personAbsenceRepository = MockRepository.GenerateMock<IPersonAbsenceRepository>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			target = new PeopleSearchProvider(searchRepository, personRepository,
				new FakePermissionProvider(), optionalColumnRepository, personAbsenceRepository, loggedOnUser);
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
			person.EmploymentNumber = "1011";

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
			personRepository.Stub(x => x.FindPeople(new List<Guid> { personId })).IgnoreArguments().Return(new List<IPerson> { person });

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
			var result = target.SearchPermittedPeopleSummary(searchCriteria, 10, 1, DateOnly.Today, new Dictionary<string, bool>(),DefinedRaptorApplicationFunctionPaths.WebPeople);

			var peopleList = result.People;
			var optionalColumns = result.OptionalColumns;
			optionalColumns.Count().Should().Be.EqualTo(1);
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
			personRepository.Stub(x => x.FindPeople(new List<Guid>())).IgnoreArguments().Return(new List<IPerson>());
			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>());

			var searchCriteria = new Dictionary<PersonFinderField, string>
			{
				{
					PersonFinderField.All, "Ashley"
				}
			};
			var result = target.SearchPermittedPeopleSummary(searchCriteria, 10, 1, DateOnly.Today, new Dictionary<string, bool>(), DefinedRaptorApplicationFunctionPaths.WebPeople);
			var peopleList = result.People;
			peopleList.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSearchPermittedPeopleWithAbsence()
		{
			personRepository.Stub(x => x.FindPeople(new List<Guid>())).IgnoreArguments().Return(new List<IPerson>());
			personAbsenceRepository.Stub(x => x.Find(new List<IPerson>(), new DateTimePeriod())).IgnoreArguments().Return(new List<IPersonAbsence> { createPersonAbsence(new DateTimePeriod())});
			loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person());
			target = new PeopleSearchProvider(searchRepository, personRepository, new FakePermissionProvider(), optionalColumnRepository, personAbsenceRepository, loggedOnUser);

			var searchCriteria = new Dictionary<PersonFinderField, string>
			{
				{
					PersonFinderField.All, "John"
				}
			};

			var permittedPeople = target.SearchPermittedPeople(searchCriteria, DateOnly.MaxValue,
				DefinedRaptorApplicationFunctionPaths.WebPeople);
			var result = target.SearchPermittedPeopleWithAbsence(permittedPeople, DateOnly.MaxValue);
			result.Count().Should().Be.EqualTo(1);
		}

		private IPersonAbsence createPersonAbsence(DateTimePeriod dateTimePeriod)
		{
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			var person = PersonFactory.CreatePersonWithGuid("John", "Smith");
			return PersonAbsenceFactory.CreatePersonAbsence(person, scenario, dateTimePeriod);
		}
	}
}
