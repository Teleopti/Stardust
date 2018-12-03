using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.ViewModelFactory
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	[RequestsTest]
	public class RequestsViewModelFactoryTest : IIsolateSystem
	{
		public IRequestsViewModelFactory Target;
		public IPersonRequestRepository PersonRequestRepository;
		public Global.FakePermissionProvider PermissionProvider;
		public FakePeopleSearchProvider PeopleSearchProvider;
		public FakeToggleManager ToggleManager;
		public IUserCulture UserCulture;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public FakePersonRepository PersonRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;

		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public FullPermission Permission;
		public ICurrentScenario Scenario;

		public IMutateNow Now;


		private IPerson[] people;
		private readonly IAbsence absence = AbsenceFactory.CreateAbsence("absence1").WithId();
		private ITeam team;
		private IPersonPeriod personPeriod;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			isolate.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("test") { DefaultScenario = true })).For<IScenarioRepository>();

			team = createTeam();
			personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2015, 10, 1), team);
		}

		[Test]
		public void ShouldGetShiftForAbsenceRequest()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.ToList()[1].Shifts.Count().Should().Be.EqualTo(6);
		}

		[Test]
		public void ShouldGetIsNewTrueForAbsenceRequest()
		{
			Now.Is(new DateTime(2015, 10, 01, 00, 00, 00, DateTimeKind.Utc));
			var workflowControlSet =
				WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);

			var absenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 10),
				OpenForRequestsPeriod = new DateOnlyPeriod(2015, 10, 01, 2015, 10, 30)
			};
			workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);

			var person = PersonFactory.CreatePerson("test1").WithId();
			person.WorkflowControlSet = workflowControlSet;
			setUpPerson(person);

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 3));
			var personRequest = new PersonRequest(person, absenceRequest).WithId();

			PersonRequestRepository.Add(personRequest);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 3),
				EndDate = new DateOnly(2015, 10, 3),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.ToList()[0].IsNew.Should().Be.True();
		}

		[Test]
		public void ShouldGetIsPendingTrueForAbsenceRequest()
		{
			Now.Is(new DateTime(2015, 10, 01, 00, 00, 00, DateTimeKind.Utc));
			var workflowControlSet =
				WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);

			var absenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 10),
				OpenForRequestsPeriod = new DateOnlyPeriod(2015, 10, 01, 2015, 10, 30)
			};
			workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);

			var person = PersonFactory.CreatePerson("test1").WithId();
			person.WorkflowControlSet = workflowControlSet;
			setUpPerson(person);

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 3));
			var personRequest = new PersonRequest(person, absenceRequest).WithId();
			personRequest.ForcePending();

			PersonRequestRepository.Add(personRequest);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 3),
				EndDate = new DateOnly(2015, 10, 3),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.ToList()[0].IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldGetIsApprovedTrueForAbsenceRequest()
		{
			Now.Is(new DateTime(2015, 10, 01, 00, 00, 00, DateTimeKind.Utc));
			var workflowControlSet =
				WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);

			var absenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 10),
				OpenForRequestsPeriod = new DateOnlyPeriod(2015, 10, 01, 2015, 10, 30)
			};
			workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);

			var person = PersonFactory.CreatePerson("test1").WithId();
			person.WorkflowControlSet = workflowControlSet;
			setUpPerson(person);

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 3));
			var personRequest = new PersonRequest(person, absenceRequest).WithId();
			personRequest.ForcePending();
			personRequest.Approve(
				new AbsenceRequestApprovalService(Scenario.Current(), new ScheduleDictionary(Scenario.Current(), new ScheduleDateTimePeriod(new DateTimePeriod(2015, 10, 1, 2015, 10, 10)), new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission()),
					new FakeNewBusinessRuleCollection(), new DoNothingScheduleDayChangeCallBack(),
					new FakeGlobalSettingDataRepository(),
					new CheckingPersonalAccountDaysProvider(PersonAbsenceAccountRepository)),
				new PersonRequestAuthorizationCheckerForTest(), true);

			PersonRequestRepository.Add(personRequest);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 3),
				EndDate = new DateOnly(2015, 10, 3),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.ToList()[0].IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldGetIsWaitlistedTrueForAbsenceRequest()
		{
			Now.Is(new DateTime(2015, 10, 01, 00, 00, 00, DateTimeKind.Utc));
			var workflowControlSet =
				WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);

			var absenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 10),
				OpenForRequestsPeriod = new DateOnlyPeriod(2015, 10, 01, 2015, 10, 30)
			};
			workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);

			var person = PersonFactory.CreatePerson("test1").WithId();
			person.WorkflowControlSet = workflowControlSet;
			setUpPerson(person);

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 3));
			var personRequest = new PersonRequest(person, absenceRequest).WithId();
			personRequest.ForcePending();
			personRequest.Deny("waitlisted", new PersonRequestAuthorizationCheckerForTest(), null,
				PersonRequestDenyOption.AutoDeny);

			PersonRequestRepository.Add(personRequest);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 3),
				EndDate = new DateOnly(2015, 10, 3),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.ToList()[0].IsWaitlisted.Should().Be.True();
		}

		[Test]
		public void ShouldGetIsDeniedTrueForAbsenceRequest()
		{
			Now.Is(new DateTime(2015, 10, 01, 00, 00, 00, DateTimeKind.Utc));
			var workflowControlSet =
				WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), false);

			var absenceRequestOpenPeriod = new AbsenceRequestOpenRollingPeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				BetweenDays = new MinMax<int>(0, 10),
				OpenForRequestsPeriod = new DateOnlyPeriod(2015, 10, 01, 2015, 10, 30)
			};
			workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);

			var person = PersonFactory.CreatePerson("test1").WithId();
			person.WorkflowControlSet = workflowControlSet;
			setUpPerson(person);

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 3));
			var personRequest = new PersonRequest(person, absenceRequest).WithId();
			personRequest.ForcePending();
			personRequest.Deny("denied", new PersonRequestAuthorizationCheckerForTest(), null,
				PersonRequestDenyOption.AutoDeny);

			PersonRequestRepository.Add(personRequest);

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 3),
				EndDate = new DateOnly(2015, 10, 3),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.ToList()[0].IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldGetNoAbsenceAndTextRequestWhenNoTeamSelected()
		{
			shouldGetNoAbsenceAndTextRequestWhenNoTeamSupplied(new List<string>().ToArray());
		}

		[Test]
		public void ShouldGetNoAbsenceAndTextRequestWhenSelectTeamIsNull()
		{
			shouldGetNoAbsenceAndTextRequestWhenNoTeamSupplied(null);
		}

		[Test]
		public void ShouldGetRequests()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetRequestsInTeams()
		{
			PeopleSearchProvider.EnablePermittedPeopleInTeams();
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() },
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetNoRequestsInWrongTeams()
		{
			PeopleSearchProvider.EnablePermittedPeopleInTeams();

			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { Guid.NewGuid().ToString() },
				SortingOrders = new List<RequestsSortingOrder>()
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetRequestsForDay()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 1),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetRequestsForSecondDay()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 2),
				EndDate = new DateOnly(2015, 10, 2),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldOrderByNameCorrectly()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder> { RequestsSortingOrder.AgentNameAsc },
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests.ToList();

			Assert.IsTrue(referencesPerson(result.First(), people[0]));
			Assert.IsTrue(referencesPerson(result.Second(), people[1]));
			Assert.IsTrue(referencesPerson(result.Last(), people[2]));
		}


		[Test]
		public void ShouldNotSeeRequestWithoutPermission()
		{
			setUpRequests();

			PermissionProvider.Enable();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 12, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { "teamId" }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotSeeRequestBeforePermissionDate()
		{
			setUpRequests();
			var date = new DateOnly(2015, 10, 3);
			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.WebRequests, date, new PersonAuthorization
			{
				TeamId = team.Id.GetValueOrDefault(),
				SiteId = team.Site?.Id.GetValueOrDefault(),
			});

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotSeeRequestBeforePermissionDateWithTimeZone()
		{
			var agentTimeZone = TimeZoneInfoFactory.ChinaTimeZoneInfo();
			var absenceRequest = new AbsenceRequest(absence, new DateOnly(2015, 10, 2).ToDateTimePeriod(agentTimeZone));

			people = new[] { PersonFactory.CreatePerson("test1").WithId() };
			people.ForEach(setUpPerson);
			people.ForEach(p => { p.PermissionInformation.SetDefaultTimeZone(agentTimeZone); });

			PersonRequestRepository.Add(new PersonRequest(people[0], absenceRequest).WithId());

			var date = new DateOnly(2015, 10, 2);
			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.WebRequests, date, new PersonAuthorization
			{
				TeamId = team.Id.GetValueOrDefault(),
				SiteId = team.Site?.Id
			});

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 2),
				EndDate = new DateOnly(2015, 10, 2),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests;
			result.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnRequestsBelongToQueriedAgents()
		{
			setUpRequests();

			PeopleSearchProvider.Add(people.First());

			GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail { PersonId = people.First().Id.Value });

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string> { { PersonFinderField.FirstName, "test1" } },
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = new[] { "teamId" }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input).Requests.ToList();
			result.Count.Should().Be.EqualTo(1);

			Assert.IsTrue(referencesPerson(result.First(), people[0]));
		}

		[Test]
		public void ShouldGetRequestsInRequestListViewModel()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(3);
			result.TotalCount.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetRequestsByRoleDescription()
		{
			var roleDescription = "my role";
			var requests = setUpRequests().ToList();

			var fakePeopleSearchProvider = PeopleSearchProvider;
			fakePeopleSearchProvider.Add(requests[0].Person, roleDescription);
			fakePeopleSearchProvider.Add(requests[1].Person);
			fakePeopleSearchProvider.Add(requests[2].Person);

			GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail { PersonId = requests[0].Person.Id.Value });
			GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail { PersonId = requests[1].Person.Id.Value });
			GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail { PersonId = requests[2].Person.Id.Value });


			ApplicationRoleRepository.Add(new ApplicationRole { DescriptionText = roleDescription });

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
			};
			input.AgentSearchTerm.Add(PersonFinderField.Role, roleDescription);

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(1);
			result.TotalCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotSeeRequestBeforePermissionDateInRequestListViewModel()
		{
			setUpRequests().ToArray();
			var date = new DateOnly(2015, 10, 3);

			PermissionProvider.Enable();

			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.WebRequests, date, new PersonAuthorization
			{
				TeamId = team.Id.GetValueOrDefault(),
				SiteId = team.Site?.Id.GetValueOrDefault(),
			});

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetNoRequestsWithNullInput()
		{
			setUpRequests();
			var result = Target.CreateAbsenceAndTextRequestListViewModel(null);
			result.Requests.Count().Should().Be.EqualTo(0);
			result.TotalCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnRequestsBelongToQueriedAgentsInRequestListViewModel()
		{
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var site = new Site("site").WithId(Guid.NewGuid());
			var team = new Team().WithDescription(new Description("my team")).WithId(Guid.NewGuid());
			team.Site = site;
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2015, 10, 3)).WithId();
			person.PersonPeriodCollection.FirstOrDefault().Team = team;
			person.SetName(new Name("Ashely", "Ashely"));
			var request = new PersonRequest(person, absenceRequest).WithId();
			PersonRequestRepository.Add(request);
			PeopleSearchProvider.Add(person);

			GroupingReadOnlyRepository.Has(new[]
			{
				new ReadOnlyGroupPage
				{
					PageId = Group.PageMainId
				}
			}, new[]
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					PersonId = person.Id.Value,
					SiteId = site.Id.Value,
					TeamId = team.Id.Value,
					GroupName = team.SiteAndTeam
				}
			});

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string> { { PersonFinderField.FirstName, "test1" } },
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.TotalCount.Should().Be.EqualTo(1);

			Assert.IsTrue(referencesPerson(result.Requests.First(), person));
		}

		[Test]
		public void ShouldReturnPersonAccountApprovalSummary()
		{
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

		[Test]
		public void ShouldGetNoOvertimeRequestsWhenNoTeamSelected()
		{
			shouldGetNoOvertimeRequestsWhenNoTeamSupplied(new string[] { });
		}

		[Test]
		public void ShouldGetNoOvertimeRequestsWhenSelectedTeamIsNull()
		{
			shouldGetNoOvertimeRequestsWhenNoTeamSupplied(null);
		}

		[Test]
		public void ShouldGetOvertimeRequests()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateOvertimeRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(1);
			result.TotalCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetOvertimeTypeFromOvertimeRequest()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateOvertimeRequestListViewModel(input);
			result.Requests.ElementAt(0).OvertimeTypeDescription.Should().Be("test");
		}

		[Test]
		public void ShouldGetOvertimeBrokenRulesFromOvertimeRequest()
		{
			var requests = setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};
			var overtimeRequest = requests[3];
			overtimeRequest.TrySetBrokenBusinessRule(BusinessRuleFlags.MaximumOvertimeRule);
			var result = Target.CreateOvertimeRequestListViewModel(input);
			var brokenRules = result.Requests.FirstOrDefault()?.BrokenRules;
			brokenRules.Count().Should().Be(1);
			brokenRules.Contains("MaximumOvertimeRuleName").Should().Be(true);
		}

		private AbsenceAndTextRequestViewModel doPersonalAccountSummaryTest(params AccountDay[] accountDays)
		{
			setupStateHolderProxy();
			setUpRequests();

			var personAbsenceAccount = createPersonAbsenceAccount(people[1], absence);

			accountDays.ForEach((accountDay) => { personAbsenceAccount.Add(accountDay); });

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 3),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { team.Id.Value.ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
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

		private PersonRequest[] setUpRequests()
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(2015, 10, 1, 2015, 10, 6));
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var textRequest2 = new TextRequest(new DateTimePeriod(2015, 10, 2, 2015, 10, 7));
			var overtimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime),
				new DateTimePeriod(2015, 10, 1, 2015, 10, 6));

			people = new[]
			{
				PersonFactory.CreatePerson("test1").WithId(),
				PersonFactory.CreatePerson("test2").WithId(),
				PersonFactory.CreatePerson("test3").WithId(),
				PersonFactory.CreatePerson("test4").WithId()
			};
			people.ForEach(setUpPerson);

			var personRequests = new[]
			{
				new PersonRequest(people[0], textRequest1).WithId(),
				new PersonRequest(people[1], absenceRequest).WithId(),
				new PersonRequest(people[2], textRequest2).WithId(),
				new PersonRequest(people[3], overtimeRequest).WithId()
			};

			PersonRequestRepository.AddRange(personRequests);

			return personRequests;
		}

		private void setUpPerson(IPerson p)
		{
			p.AddPersonPeriod(personPeriod);
			PersonRepository.Add(p);
			PeopleSearchProvider.Add(p);

			GroupingReadOnlyRepository.Has(new ReadOnlyGroupDetail
			{
				BusinessUnitId = Guid.Empty,
				PersonId = p.Id.GetValueOrDefault(),
				TeamId = team.Id,
				SiteId = team.Site.Id
			});

			PeopleSearchProvider.Add(team.Id.GetValueOrDefault(), p);
		}

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}

		private static Team createTeam()
		{
			var team = TeamFactory.CreateSimpleTeam("_").WithId();
			team.Site = SiteFactory.CreateSimpleSite("site").WithId();
			return team;
		}

		private void shouldGetNoAbsenceAndTextRequestWhenNoTeamSupplied(string[] selectedTeams)
		{
			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SortingOrders = new List<RequestsSortingOrder>(),
				SelectedGroupIds = selectedTeams
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.TotalCount.Should().Be.EqualTo(0);
		}

		private void shouldGetNoOvertimeRequestsWhenNoTeamSupplied(string[] selectedTeams)
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = selectedTeams
			};

			var result = Target.CreateOvertimeRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(0);
		}
	}
}