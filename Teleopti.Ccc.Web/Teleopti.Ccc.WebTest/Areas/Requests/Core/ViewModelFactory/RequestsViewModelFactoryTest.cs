using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
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
		public FakeToggleManager ToggleManager;
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
				SelectedGroupIds = new List<string>().ToArray()
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.TotalCount.Should().Be.EqualTo(0);
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
				SelectedGroupIds = new []{Guid.NewGuid().ToString()}
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(3);
			result.TotalCount.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldGetRequestsByRoleDescription()
		{
			ToggleManager.Enable(Toggles.Wfm_GroupPages_45057);
			var roleDescription = "my role";
			var requests = setUpRequests().ToList();

			var fakePeopleSearchProvider = (FakePeopleSearchProvider) PeopleSearchProvider;
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
			input.AgentSearchTerm.Add(PersonFinderField.Role, $"\"{roleDescription}\"");

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(1);
			result.TotalCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotSeeRequestBeforePermissionDateInRequestListViewModel()
		{
			var requests = setUpRequests().ToArray();
			var date = new DateOnly(2015, 10, 3);

			var permissionProvider = PermissionProvider as Global.FakePermissionProvider;
			permissionProvider.Enable();

			permissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.WebRequests, date, new PersonAuthorization
			{
				PersonId = requests[0].Person.Id.GetValueOrDefault(),
				TeamId = requests[0].Person.MyTeam(date)?.Id.GetValueOrDefault(),
				SiteId = requests[0].Person.MyTeam(date)?.Site?.Id.GetValueOrDefault(),
			});
			permissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.WebRequests, date, new PersonAuthorization
			{
				PersonId = requests[1].Person.Id.GetValueOrDefault(),
				TeamId = requests[1].Person.MyTeam(date)?.Id.GetValueOrDefault(),
				SiteId = requests[1].Person.MyTeam(date)?.Site?.Id.GetValueOrDefault(),
			});
			permissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.WebRequests, date, new PersonAuthorization
			{
				PersonId = requests[2].Person.Id.GetValueOrDefault(),
				TeamId = requests[2].Person.MyTeam(date)?.Id.GetValueOrDefault(),
				SiteId = requests[2].Person.MyTeam(date)?.Site?.Id.GetValueOrDefault(),
			});

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 31),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
			};

			var result = Target.CreateAbsenceAndTextRequestListViewModel(input);
			result.Requests.Count().Should().Be.EqualTo(1);
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
			var personSearchProvider = PeopleSearchProvider as FakePeopleSearchProvider;
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var site = new Site("site").WithId(Guid.NewGuid());
			var team = new Team().WithDescription(new Description("my team")).WithId(Guid.NewGuid());
			team.Site = site;
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2015, 10, 3)).WithId();
			person.PersonPeriodCollection.FirstOrDefault().Team = team;
			person.SetName(new Name("Ashely", "Ashely"));
			var request = new PersonRequest(person, absenceRequest).WithId();
			PersonRequestRepository.Add(request);
			personSearchProvider.Add(person);

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
				SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
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
		public void ShouldGetOvertimeRequests()
		{
			setUpRequests();

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2015, 10, 1),
				EndDate = new DateOnly(2015, 10, 9),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
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
				SelectedGroupIds = new[] {Guid.NewGuid().ToString()}
			};

			var result = Target.CreateOvertimeRequestListViewModel(input);
			result.Requests.ElementAt(0).OvertimeTypeDescription.Should().Be("test");
		}

		private AbsenceAndTextRequestViewModel doPersonalAccountSummaryTest(params AccountDay[] accountDays)
		{
			setupStateHolderProxy();
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
				SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
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

		private IEnumerable<IPersonRequest> setUpRequests()
		{
			var textRequest1 = new TextRequest(new DateTimePeriod(2015, 10, 1, 2015, 10, 6));
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var textRequest2 = new TextRequest(new DateTimePeriod(2015, 10, 2, 2015, 10, 7));
			var overtimeRequest = new OvertimeRequest(new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime),
				new DateTimePeriod(2015, 10, 1, 2015, 10, 6));

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
				 new PersonRequest(people[2], textRequest2).WithId(),
				 new PersonRequest(people[0], overtimeRequest).WithId()
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

