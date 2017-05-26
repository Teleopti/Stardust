using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
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
				SelectedTeamIds = new List<Guid>().ToArray()
			};

			var result = Target.CreateRequestListViewModel(input);
			result.TotalCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetRequests()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetRequestsInTeams()
		{
			((FakeToggleManager)ToggleManager).Enable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			var team = TeamFactory.CreateSimpleTeam("_").WithId();
			team.Site = SiteFactory.CreateSimpleSite("site");
			setUpRequests(team);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new []{team.Id.Value},
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetNoRequestsInWrongTeams()
		{
			((FakeToggleManager)ToggleManager).Enable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			var team = TeamFactory.CreateSimpleTeam("_").WithId();
			team.Site = SiteFactory.CreateSimpleSite("site");
			setUpRequests(team);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new []{Guid.NewGuid()},
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetRequestsForDay()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 1),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetRequestsForSecondDay()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 2),
				EndDate = new DateOnly(2015, 10, 2),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldOrderByNameCorrectly()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder> { RequestsSortingOrder.AgentNameAsc }
			};

			var result = Target.Create(input).ToList();

			Assert.IsTrue(referencesPerson(result.First(), people[0]));
			Assert.IsTrue(referencesPerson(result.Second(), people[1]));
			Assert.IsTrue(referencesPerson(result.Last(), people[2]));
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
		public void ShouldNotSeeRequestBeforePermissionDate()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			setUpRequests();

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();
			permissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebRequests, new DateOnly(2015, 10, 3));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input);
			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnRequestsBelongToQueriedAgents()
		{
			var personSearchProvider = PeopleSearchProvider as FakePeopleSearchProvider;
			var requests = setUpRequests();

			personSearchProvider.Add(requests.First().Person );

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string> { { PersonFinderField.FirstName, "test1" } },
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.Create(input).ToList();
			result.Count.Should().Be.EqualTo(1);

			Assert.IsTrue(referencesPerson(result.First(), people[0]));
		}

		[Test]
		public void ShouldGetRequestsInRequestListViewModel()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new []{Guid.NewGuid()}
			};

			var result = Target.CreateRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(3);
			result.TotalCount.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetRequestsByRoleDescription()
		{
			var roleDescription = "my role";
			var requests = setUpRequests().ToList();
			var fakePeopleSearchProvider = (FakePeopleSearchProvider) PeopleSearchProvider;
			fakePeopleSearchProvider.Add(requests[0].Person, roleDescription);
			fakePeopleSearchProvider.Add(requests[1].Person);
			fakePeopleSearchProvider.Add(requests[2].Person);

			ApplicationRoleRepository.Add(new ApplicationRole { DescriptionText = roleDescription });

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new[] { Guid.NewGuid() }
			};
			input.AgentSearchTerm.Add(PersonFinderField.Role, roleDescription);

			var result = Target.CreateRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(1);
			result.TotalCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotSeeRequestBeforePermissionDateInRequestListViewModel()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			setUpRequests();

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			permissionProvider.Permit(DefinedRaptorApplicationFunctionPaths.WebRequests, new DateOnly(2015, 10, 3));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new[] { Guid.NewGuid() }
			};

			var result = Target.CreateRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetNoRequestsWithNullInput()
		{
			setUpRequests();
			var result = Target.CreateRequestListViewModel(null);
			result.Requests.Count().Should().Be.EqualTo(0);
			result.TotalCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnRequestsBelongToQueriedAgentsInRequestListViewModel()
		{
			var personSearchProvider = PeopleSearchProvider as FakePeopleSearchProvider;
			var requests = setUpRequests();

			personSearchProvider.Add(requests.First().Person);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string> { { PersonFinderField.FirstName, "test1" } },
				SelectedTeamIds = new[] { Guid.NewGuid() }
			};

			var result = Target.CreateRequestListViewModel(input);
			result.TotalCount.Should().Be.EqualTo(1);

			Assert.IsTrue(referencesPerson(result.Requests.First(), people[0]));

		}

		[Test]
		public void ShouldReturnPersonAccountApprovalSummary()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			var accountDay = new AccountDay(new DateOnly(2015, 1, 1))
			{
				BalanceIn = TimeSpan.FromDays(0),
				Accrued = TimeSpan.FromDays(2),
				Extra = TimeSpan.FromDays(0)
			};
			
			var accountDay2 = new AccountDay(new DateOnly(2015, 10, 5))
			{
				BalanceIn = TimeSpan.FromDays(0),
				Accrued = TimeSpan.FromDays(4),
				Extra = TimeSpan.FromDays(0)
			};

			var absenceRequest = doPersonalAccountSummaryTest(accountDay, accountDay2);

			var firstSummaryDetail = absenceRequest.PersonAccountSummaryViewModel.PersonAccountSummaryDetails.First();
			var secondSummaryDetail = absenceRequest.PersonAccountSummaryViewModel.PersonAccountSummaryDetails.Second();

			Assert.AreEqual(accountDay.StartDate.Date, firstSummaryDetail.StartDate);
			Assert.AreEqual(accountDay.Remaining.TotalDays.ToString(UserCulture.GetCulture()), firstSummaryDetail.RemainingDescription);
			Assert.AreEqual(Resources.Days, firstSummaryDetail.TrackingTypeDescription);
			Assert.AreEqual(accountDay2.StartDate.Date, secondSummaryDetail.StartDate);
			Assert.AreEqual(accountDay2.Remaining.TotalDays.ToString(UserCulture.GetCulture()), secondSummaryDetail.RemainingDescription);
			Assert.AreEqual(Resources.Days, secondSummaryDetail.TrackingTypeDescription);
		}

		[Test]
		public void ShouldNotReturnPersonAccountOutsideOfRequestPeriodInApprovalSummary()
		{
			((FakeToggleManager)ToggleManager).Disable(Toggles.Wfm_Requests_DisplayRequestsOnBusinessHierachy_42309);
			var accountDay = new AccountDay(new DateOnly(2015, 1, 1))
			{
				BalanceIn = TimeSpan.FromDays(0),
				Accrued = TimeSpan.FromDays(2),
				Extra = TimeSpan.FromDays(0)
			};

			var accountDay2 = new AccountDay(new DateOnly(2016, 10, 5))
			{
				BalanceIn = TimeSpan.FromDays(0),
				Accrued = TimeSpan.FromDays(4),
				Extra = TimeSpan.FromDays(0)
			};

			var absenceRequest = doPersonalAccountSummaryTest(accountDay, accountDay2);

			var summaryDetails = absenceRequest.PersonAccountSummaryViewModel.PersonAccountSummaryDetails;
			var firstSummaryDetail = summaryDetails.First();
			Assert.AreEqual(1, summaryDetails.Count());
			Assert.AreEqual(accountDay.StartDate.Date, firstSummaryDetail.StartDate);
		}

		private RequestViewModel doPersonalAccountSummaryTest(params AccountDay[] accountDays)
		{

			setupStateHolderProxy();

			((FakeToggleManager)ToggleManager).Enable(Toggles.Wfm_Requests_Show_Personal_Account_39628);

			setUpRequests();

			var personAbsenceAccount = createPersonAbsenceAccount(people[1], absence);

			accountDays.ForEach((accountDay) =>
		   {
			   personAbsenceAccount.Add(accountDay);
		   });


			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 3),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new[] { Guid.NewGuid() }
			};

			var result = Target.CreateRequestListViewModel(input);
			var absenceRequest = result.Requests.Single(model => model.Type == RequestType.AbsenceRequest);
			return absenceRequest;
		}

		private static bool referencesPerson(RequestViewModel requestViewModel, IPerson person)
		{
			return requestViewModel.AgentName == person.Name.ToString() && requestViewModel.PersonId == person.Id;
		}


		private PersonAbsenceAccount createPersonAbsenceAccount(IPerson person, IAbsence absence)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();

			PersonAbsenceAccountRepository.Add(personAbsenceAccount);

			return personAbsenceAccount;
		}

		private IEnumerable<IPersonRequest> setUpRequests()
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(2015, 10, 1, 2015, 10, 6));
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var textRequest2 = new TextRequest(new DateTimePeriod(2015, 10, 2, 2015, 10, 7));

			people = new[]
			{
				PersonFactory.CreatePerson ("test1").WithId(),
				PersonFactory.CreatePerson ("test2").WithId(),
				PersonFactory.CreatePerson ("test3").WithId()
			}
			.ToList();

			var personRequests = new[]
			{
				 new PersonRequest(people[0], textRequest1).WithId(),
				 new PersonRequest(people[1], absenceRequest).WithId(),
				 new PersonRequest (people[2], textRequest2).WithId()
			};

			PersonRequestRepository.AddRange(personRequests);

			return personRequests.ToList();
		}

		private void setUpRequests(ITeam team)
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(2015, 10, 1, 2015, 10, 6));
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var textRequest2 = new TextRequest(new DateTimePeriod(2015, 10, 2, 2015, 10, 7));
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2015, 10, 1), team);

			people = new[]
			{
				PersonFactory.CreatePerson ("test1").WithId(),
				PersonFactory.CreatePerson ("test2").WithId(),
				PersonFactory.CreatePerson ("test3").WithId()
			}
			.ToList();
			people.ForEach(p =>
			{
				p.AddPersonPeriod(personPeriod);
				((FakePeopleSearchProvider)PeopleSearchProvider).Add(p);
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
				 new PersonRequest (people[2], textRequest2).WithId()
			};

			PersonRequestRepository.AddRange(personRequests);
		}

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}
	}
}

