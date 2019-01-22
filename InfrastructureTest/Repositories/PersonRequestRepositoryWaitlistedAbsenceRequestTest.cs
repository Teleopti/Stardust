using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class PersonRequestRepositoryWaitlistedAbsenceRequestTest : RepositoryTest<IPersonRequest>
	{
		private IPerson _person;
		private IAbsence _absence;
		private IScenario _defaultScenario;
		private Team _team;
		private Site _site;
		private Contract _contract;
		private PartTimePercentage _partTimePercentage;
		private IContractSchedule _contractSchedule;
		private DateTime _baseTime;

		protected override void ConcreteSetup()
		{
			_baseTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			_defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
			_person = PersonFactory.CreatePerson("sdfoj");
			_absence = AbsenceFactory.CreateAbsence("Sick leave");

			_team = TeamFactory.CreateSimpleTeam("team");
			_site = SiteFactory.CreateSimpleSite("site");
			_team.Site = _site;
			_contract = new Contract("contract");
			_partTimePercentage = new PartTimePercentage("partTimePercentage");
			_contractSchedule = ContractScheduleFactory.CreateContractSchedule("contractSchedule");

			PersistAndRemoveFromUnitOfWork(_site);
			PersistAndRemoveFromUnitOfWork(_team);
			PersistAndRemoveFromUnitOfWork(_contract);
			PersistAndRemoveFromUnitOfWork(_partTimePercentage);
			PersistAndRemoveFromUnitOfWork(_contractSchedule);

			PersistAndRemoveFromUnitOfWork(_defaultScenario);
			PersistAndRemoveFromUnitOfWork(_person);
			PersistAndRemoveFromUnitOfWork(_absence);
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsBasedOnWaitlistedAndAutoGrantPendingStatus()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);

				PersistAndRemoveFromUnitOfWork(wcs);

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("p1", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(person1);

				var period1 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request1 = PersonRequestFactory.CreateNewPersonRequest(person1, absence, period1, "new 1");
				var request2 = PersonRequestFactory.CreateNewPersonRequest(person1, absence, period1, "new 2");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person1, absence, period2, "pending 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(11), _baseTime.AddHours(20));
				var request4 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period3, "Waitlisted 1");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request2,
					request3,
					request4
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period3, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request4.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsOrderByCreateTime()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);
				PersistAndRemoveFromUnitOfWork(wcs);

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("-", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork( person1 );

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(16));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person1, absence, period3, "Pending");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period2, "Waitlisted 2");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request3,
					request2
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(new DateTimePeriod(_baseTime, _baseTime.AddDays(1).AddSeconds(-1)), budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsOrderByPersonSeniorityThenCreateTime()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs = WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
					WaitlistProcessOrder.BySeniority);
				PersistAndRemoveFromUnitOfWork(wcs);

				var budgetGroup1 = new BudgetGroup {Name = "group1", TimeZone = TimeZoneInfo.Utc};
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("p1", team1, budgetGroup1, wcs,
					new List<DateOnly>
					{
						new DateOnly(2016, 03, 01),
						new DateOnly(2016, 03, 02)
					});
				var person2 = createPerson("p2", team1, budgetGroup1, wcs,
					new List<DateOnly>
					{
						new DateOnly(2016, 03, 01),
						new DateOnly(2016, 03, 05)
					});
				var person3 = createPerson("p3", team1, budgetGroup1, wcs,
					new List<DateOnly>
					{
						new DateOnly(2016, 02, 03),
						new DateOnly(2016, 03, 04)
					});
				PersistAndRemoveFromUnitOfWork(new[] {person1, person2, person3});

				var period1 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(12));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(12));
				var request2 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person2, absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(11), _baseTime.AddHours(18));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person3, absence, period3, "Pending");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request3,
					request2,
					request1
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period3, budgetGroup1.Id.Value,
						WaitlistProcessOrder.BySeniority).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest
						{PersonRequestId = request3.Id.Value, RequestStatus = PersonRequestStatus.Pending},
					new PersonWaitlistedAbsenceRequest
						{PersonRequestId = request2.Id.Value, RequestStatus = PersonRequestStatus.Waitlisted},
					new PersonWaitlistedAbsenceRequest
						{PersonRequestId = request1.Id.Value, RequestStatus = PersonRequestStatus.Waitlisted},
				}));
			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsWhenAbsencePeriodsNotOverlap()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);
				PersistAndRemoveFromUnitOfWork( wcs );

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("p1", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(person1);

				var period1 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(11));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");
				
				var period2 = new DateTimePeriod(_baseTime.AddHours(13), _baseTime.AddHours(16));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(14));
				var request3 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period3, "Waitlisted 3");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request2,
					request3
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period2, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsWhenAbsencePeriodsOverlap()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);

				PersistAndRemoveFromUnitOfWork( wcs );

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("p1", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(person1);

				var period1 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(11));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");

				var period2 = new DateTimePeriod(_baseTime.AddHours(13), _baseTime.AddHours(16));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(14));
				var request3 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period3, "Waitlisted 3");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request2,
					request3
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period3, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsWhenAbsencePeriodEdgeTouching()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);
				PersistAndRemoveFromUnitOfWork(wcs);

				var budgetGroup1 = new BudgetGroup {Name = "group1", TimeZone = TimeZoneInfo.Utc};
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("p1", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(person1);

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");

				var period2 = new DateTimePeriod(_baseTime.AddHours(19), _baseTime.AddHours(23));
				var request2 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(15));
				var request3 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period3, "Waitlisted 3");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request3,
					request2
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period3, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(1);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest
						{PersonRequestId = request3.Id.Value, RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsIfPersonHasNoWorkflowControlSet()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var waitlistedWcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);

				PersistAndRemoveFromUnitOfWork(new[] { waitlistedWcs });

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("person1", team1, budgetGroup1, waitlistedWcs, new List<DateOnly>{ new DateOnly(2016, 03, 01) });
				var person2 = createPerson("person2", team1, budgetGroup1, null, new List<DateOnly> { new DateOnly(2016, 03, 01) });
				PersistAndRemoveFromUnitOfWork(new[] { person1, person2 });

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(14));
				var request2 = PersonRequestFactory.CreatePendingPersonRequest(person2, absence, period2, "Pending");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request2,
					request1
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period2, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(0);
			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsWhenWorkflowControlSetIsDeleted()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);

				PersistAndRemoveFromUnitOfWork(new[] { wcs });

				var budgetGroup1 = new BudgetGroup {Name = "group1", TimeZone = TimeZoneInfo.Utc};
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("person1", team1, budgetGroup1, wcs,
					new List<DateOnly> {new DateOnly(2016, 03, 01)});
				PersistAndRemoveFromUnitOfWork(person1);

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted");
				PersistAndRemoveFromUnitOfWork(request1);

				wcs.SetDeleted();
				PersistAndRemoveFromUnitOfWork(wcs);

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period1, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(0);
			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsIfPersonIsDeleted()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var waitlistedWcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);

				PersistAndRemoveFromUnitOfWork(new[] {waitlistedWcs});

				var budgetGroup1 = new BudgetGroup {Name = "group1", TimeZone = TimeZoneInfo.Utc};
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("person1", team1, budgetGroup1, waitlistedWcs,
					new List<DateOnly> {new DateOnly(2016, 03, 01)});
				var person2 = createPerson("person2", team1, budgetGroup1, waitlistedWcs,
					new List<DateOnly> {new DateOnly(2016, 03, 01)});
				PersistAndRemoveFromUnitOfWork(new[] {person1, person2});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreatePendingPersonRequest(person2, absence, period2, "Pending");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request2
				});

				((Person)person1).SetDeleted();
				PersistAndRemoveFromUnitOfWork(person1);

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period1, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(1);

				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest
						{PersonRequestId = request2.Id.Value, RequestStatus = PersonRequestStatus.Pending},
				}));
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsRegardlessWorkflowControlSet()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);
				PersistAndRemoveFromUnitOfWork(wcs);

				var wcs2 =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);
				PersistAndRemoveFromUnitOfWork(wcs2);

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("-", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(person1);

				var person2 = createPerson("-", team1, budgetGroup1, wcs2, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(person2);

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(16));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person1, absence, period3, "Pending");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person2, absence, period2, "Waitlisted 2");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request3,
					request2
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period3, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsBasedOnBudgetGroups()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);
				PersistAndRemoveFromUnitOfWork(wcs);

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				var budgetGroup2 = new BudgetGroup { Name = "group2", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(new []{budgetGroup1,budgetGroup2});

				var person1 = createPerson("-", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				var person2 = createPerson("-", team1, budgetGroup2, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(new[] {person1, person2});

				var period1 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request3 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period3, "Waitlisted 3");

				var period2 = new DateTimePeriod(_baseTime.AddHours(11), _baseTime.AddHours(20));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person2, absence, period2, "Waitlisted 2");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request3,
					request2
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period1, budgetGroup1.Id.Value).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsRegardlessBudgetGroupWhenAgentHasNoBudgetGroup()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var team1 = TeamFactory.CreateSimpleTeam("team1");
				team1.Site = _site;
				PersistAndRemoveFromUnitOfWork(team1);

				var absence = AbsenceFactory.CreateAbsence("Football");
				PersistAndRemoveFromUnitOfWork(absence);

				var wcs =
					WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(absence,
						WaitlistProcessOrder.FirstComeFirstServed);
				PersistAndRemoveFromUnitOfWork(wcs);

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				PersistAndRemoveFromUnitOfWork(budgetGroup1);

				var person1 = createPerson("-", team1, budgetGroup1, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				var person2 = createPerson("-", team1, null, wcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				PersistAndRemoveFromUnitOfWork(new [] {person1 , person2});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(16));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person2, absence, period3, "Pending");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person2, absence, period2, "Waitlisted 2");

				PersistPersonRequestListInOrder(new List<IPersonRequest>
				{
					request1,
					request3,
					request2
				});

				var pendingAndWaitlistedRequests = new PersonRequestRepository(UnitOfWork)
					.GetPendingAndWaitlistedAbsenceRequests(period2, null).ToArray();

				pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
				Assert.True(SequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
					new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
				}));
			}
		}


		private IPerson createPerson(string personName, Team team1, BudgetGroup budgetGroup1, WorkflowControlSet wcs, List<DateOnly> personPeriods)
		{
			var person1 = PersonFactory.CreatePerson(personName);

			if (wcs != null)
				person1.WorkflowControlSet = wcs;

			foreach (var personPeriod in personPeriods)
			{
				person1.AddPersonPeriod(
					PersonPeriodFactory.CreatePersonPeriod(personPeriod,
						PersonContractFactory.CreatePersonContract(_contract, _partTimePercentage, _contractSchedule),
						team1, budgetGroup1));
			}

			return person1;
		}

		private bool assertEquals(PersonWaitlistedAbsenceRequest first, PersonWaitlistedAbsenceRequest second)
		{
			return first.PersonRequestId == second.PersonRequestId && first.RequestStatus == second.RequestStatus;
		}
		
		private void PersistPersonRequestListInOrder(List<IPersonRequest> requests)
		{
			foreach (var request in requests)
			{
				Thread.Sleep(10);
				PersistAndRemoveFromUnitOfWork(request);
			}
		}

		private bool SequenceEquals(IList<PersonWaitlistedAbsenceRequest> actualList,
			IList<PersonWaitlistedAbsenceRequest> expectedList)
		{
			if (expectedList.Count != actualList.Count)
				return false;

			bool result = true;
			for (int i=0; i< actualList.Count;i++)
			{
				result = result && assertEquals(expectedList[i], actualList[i]);
			}

			return result;
		}

		protected override IPersonRequest CreateAggregateWithCorrectBusinessUnit()
		{
			var period = new DateTimePeriod(
				new DateTime(2008, 7, 10, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 7, 11, 0, 0, 0, DateTimeKind.Utc));
			IPersonRequest request = new PersonRequest(_person);
			IAbsenceRequest absenceRequest = new AbsenceRequest(_absence, period);

			request.Request = absenceRequest;
			request.Pending();

			return request;
		}

		protected override void VerifyAggregateGraphProperties(IPersonRequest loadedAggregateFromDatabase)
		{
			IPersonRequest org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.Person, loadedAggregateFromDatabase.Person);
			Assert.AreEqual(((IAbsenceRequest)org.Request).Absence,
				((IAbsenceRequest)loadedAggregateFromDatabase.Request).Absence);
			Assert.AreEqual((org.Request).Period,
				(loadedAggregateFromDatabase.Request).Period);
		}

		protected override Repository<IPersonRequest> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonRequestRepository(currentUnitOfWork);
		}
	}
}