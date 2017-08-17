using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.ViewModelFactory
{
	[TestFixture, RequestsTest]
	public class RequestsViewModelFactoryTest : ISetup
	{
		public IRequestsViewModelFactory Target;
		public IPersonRequestRepository PersonRequestRepository;
		public IPermissionProvider PermissionProvider;
		public IPeopleSearchProvider PeopleSearchProvider;
		public IPersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public IToggleManager ToggleManager;
		public IUserCulture UserCulture;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public FakePersonRepository PersonRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;

		private List<IPerson> people;
		private readonly IAbsence absence = AbsenceFactory.CreateAbsence("absence1").WithId();

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
		}

		[Test]
		public void ShouldGetNothingWhenSelectNoAnyTeam()
		{
			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new List<Guid>().ToArray()
			};

			var result = Target.CreateRequestListViewModel(input);
			result.TotalCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetRequestsInTeams()
		{
			var team = TeamFactory.CreateSimpleTeam("_").WithId();
			team.Site = SiteFactory.CreateSimpleSite("site");
			setUpRequests(team);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] {team.Id.Value},
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetNoRequestsInWrongTeams()
		{
			var team = TeamFactory.CreateSimpleTeam("_").WithId();
			team.Site = SiteFactory.CreateSimpleSite("site");
			setUpRequests(team);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] {Guid.NewGuid()},
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotSeeRequestWithoutPermission()
		{
			setUpRequests();

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetNoRequestsWithNullInput()
		{
			setUpRequests();
			var result = Target.CreateRequestListViewModel(null);
			result.Requests.Count().Should().Be.EqualTo(0);
			result.TotalCount.Should().Be.EqualTo(0);
		}

		private void setUpRequests()
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(2015, 10, 1, 2015, 10, 6));
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var textRequest2 = new TextRequest(new DateTimePeriod(2015, 10, 2, 2015, 10, 7));

			people = new[]
				{
					PersonFactory.CreatePerson("test1").WithId(),
					PersonFactory.CreatePerson("test2").WithId(),
					PersonFactory.CreatePerson("test3").WithId()
				}
				.ToList();

			var personRequests = new[]
			{
				new PersonRequest(people[0], textRequest1).WithId(),
				new PersonRequest(people[1], absenceRequest).WithId(),
				new PersonRequest(people[2], textRequest2).WithId()
			};

			PersonRequestRepository.AddRange(personRequests);
		}

		private void setUpRequests(ITeam team)
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(2015, 10, 1, 2015, 10, 6));
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var textRequest2 = new TextRequest(new DateTimePeriod(2015, 10, 2, 2015, 10, 7));
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2015, 10, 1), team);

			people = new[]
				{
					PersonFactory.CreatePerson("test1").WithId(),
					PersonFactory.CreatePerson("test2").WithId(),
					PersonFactory.CreatePerson("test3").WithId()
				}
				.ToList();
			people.ForEach(p =>
			{
				p.AddPersonPeriod(personPeriod);
				((FakePeopleSearchProvider) PeopleSearchProvider).Add(p);
				PersonRepository.Add(p);

				GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail
				{
					BusinessUnitId = Guid.Empty,
					PersonId = p.Id.GetValueOrDefault(),
					TeamId = team.Id.GetValueOrDefault(),
					SiteId = team.Site.Id.GetValueOrDefault()
				});
			});

			var personRequests = new[]
			{
				new PersonRequest(people[0], textRequest1).WithId(),
				new PersonRequest(people[1], absenceRequest).WithId(),
				new PersonRequest(people[2], textRequest2).WithId()
			};

			PersonRequestRepository.AddRange(personRequests);
		}
	}
}
