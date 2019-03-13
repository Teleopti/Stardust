using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class PersonRequestRepositoryWaitlistedAbsenceRequestTest
	{
		public WithUnitOfWork WithUnitOfWork;
		public IAbsenceRepository AbsenceRepository;
		public IActivityRepository ActivityRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public ISkillRepository SkillRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IContractRepository ContractRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IPersonRepository PersonRepository;
		public IBudgetGroupRepository BudgetGroupRepository;
		public MutableNow Now;
		public IPersonRequestRepository Target;

		private readonly DateTime _baseTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
		private IPerson _person;
		private IAbsence _absence;
		private IScenario _defaultScenario;
		private Team _team;
		private Site _site;
		private Contract _contract;
		private PartTimePercentage _partTimePercentage;
		private IContractSchedule _contractSchedule;
		private WorkflowControlSet _waitlistedWcs;
		private BudgetGroup _budgetGroup;

		[Test]
		public void ShouldNotReturnDeletedWaitlistedRequests()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("person1", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly> { new DateOnly(2016, 03, 01) });

				WithUnitOfWork.Do(() =>
				{
					BudgetGroupRepository.Add(_budgetGroup);
					PersonRepository.Add(person1);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period2, "Waitlisted");

				persistPersonRequests(request1, request2);

				((PersonRequest)request1).SetDeleted();
				persistPersonRequests(request1);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target.GetPendingAndWaitlistedAbsenceRequests(period2, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(1);

					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
						{
							new PersonWaitlistedAbsenceRequest
								{PersonRequestId = request2.Id.Value, RequestStatus = PersonRequestStatus.Waitlisted},
						}));
				});
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsBasedOnWaitlistedAndAutoGrantPendingStatus()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("p1", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request1 = PersonRequestFactory.CreateNewPersonRequest(person1, _absence, period1, "new 1");
				var request2 = PersonRequestFactory.CreateNewPersonRequest(person1, _absence, period1, "new 2");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person1, _absence, period2, "pending 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(11), _baseTime.AddHours(20));
				var request4 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period3, "Waitlisted 1");

				persistPersonRequests(
					request1,
					request2,
					request3,
					request4);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target.GetPendingAndWaitlistedAbsenceRequests(period3, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request4.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
					}));
				});
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsOrderByCreateTime()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("-", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(16));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person1, _absence, period3, "Pending");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period2, "Waitlisted 2");

				persistPersonRequests(
					request1,
					request3,
					request2
				);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target
					.GetPendingAndWaitlistedAbsenceRequests(new DateTimePeriod(_baseTime, _baseTime.AddDays(1).AddSeconds(-1)), _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
						{
							new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
							new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
							new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
						}));
				});

			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsOrderByPersonSeniorityThenCreateTime()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("p1", _team, _budgetGroup, _waitlistedWcs,
					new List<DateOnly>
					{
						new DateOnly(2016, 03, 01),
						new DateOnly(2016, 03, 02)
					});
				var person2 = createPerson("p2", _team, _budgetGroup, _waitlistedWcs,
					new List<DateOnly>
					{
						new DateOnly(2016, 03, 01),
						new DateOnly(2016, 03, 05)
					});
				var person3 = createPerson("p3", _team, _budgetGroup, _waitlistedWcs,
					new List<DateOnly>
					{
						new DateOnly(2016, 02, 03),
						new DateOnly(2016, 03, 04)
					});
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
					PersonRepository.Add(person2);
					PersonRepository.Add(person3);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(12));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(12));
				var request2 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person2, _absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(11), _baseTime.AddHours(18));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person3, _absence, period3, "Pending");

				persistPersonRequests(
					request3,
					request2,
					request1);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target
					.GetPendingAndWaitlistedAbsenceRequests(period3, _budgetGroup.Id.Value,
						WaitlistProcessOrder.BySeniority).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest
							{PersonRequestId = request3.Id.Value, RequestStatus = PersonRequestStatus.Pending},
						new PersonWaitlistedAbsenceRequest
							{PersonRequestId = request2.Id.Value, RequestStatus = PersonRequestStatus.Waitlisted},
						new PersonWaitlistedAbsenceRequest
							{PersonRequestId = request1.Id.Value, RequestStatus = PersonRequestStatus.Waitlisted},
					}));
				});

			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsWhenAbsencePeriodsNotOverlap()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("p1", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(11));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period2 = new DateTimePeriod(_baseTime.AddHours(13), _baseTime.AddHours(16));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(14));
				var request3 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period3, "Waitlisted 3");

				persistPersonRequests(
					request1,
					request2,
					request3
				);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target
					.GetPendingAndWaitlistedAbsenceRequests(period2, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
					}));
				});

			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsWhenAbsencePeriodsOverlap()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("p1", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(11));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period2 = new DateTimePeriod(_baseTime.AddHours(13), _baseTime.AddHours(16));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(14));
				var request3 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period3, "Waitlisted 3");

				persistPersonRequests(
					request1,
					request2,
					request3
				);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target
					.GetPendingAndWaitlistedAbsenceRequests(period3, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
					}));
				});

			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsWhenAbsencePeriodEdgeTouching()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("p1", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period2 = new DateTimePeriod(_baseTime.AddHours(19), _baseTime.AddHours(23));
				var request2 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period2, "Waitlisted 2");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(15));
				var request3 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period3, "Waitlisted 3");

				persistPersonRequests(
					request1,
					request3,
					request2
				);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target
					.GetPendingAndWaitlistedAbsenceRequests(period3, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(1);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
				{
					new PersonWaitlistedAbsenceRequest
						{PersonRequestId = request3.Id.Value, RequestStatus = PersonRequestStatus.Waitlisted}
				}));
				});

			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsIfPersonHasNoWorkflowControlSet()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();
				var person1 = createPerson("person1", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly> { new DateOnly(2016, 03, 01) });
				var person2 = createPerson("person2", _team, _budgetGroup, null, new List<DateOnly> { new DateOnly(2016, 03, 01) });
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
					PersonRepository.Add(person2);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(14));
				var request2 = PersonRequestFactory.CreatePendingPersonRequest(person2, _absence, period2, "Pending");

				persistPersonRequests(
					request2,
					request1
				);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target
					.GetPendingAndWaitlistedAbsenceRequests(period2, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(0);
				});
			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsWhenWorkflowControlSetIsDeleted()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("person1", _team, _budgetGroup, _waitlistedWcs,
					new List<DateOnly> { new DateOnly(2016, 03, 01) });
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted");
				persistPersonRequests(request1);

				WithUnitOfWork.Do((uow) =>
				{
					((IDeleteTag)WorkflowControlSetRepository.Get(_waitlistedWcs.Id.Value)).SetDeleted();
				});

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target.GetPendingAndWaitlistedAbsenceRequests(period1, _budgetGroup.Id.Value).ToArray();
					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(0);
				});
			}
		}

		[Test]
		public void ShouldNotReturnWaitlistedRequestsIfPersonIsDeleted()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("person1", _team, _budgetGroup, _waitlistedWcs,
					new List<DateOnly> { new DateOnly(2016, 03, 01) });
				var person2 = createPerson("person2", _team, _budgetGroup, _waitlistedWcs,
					new List<DateOnly> { new DateOnly(2016, 03, 01) });
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
					PersonRepository.Add(person2);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 =
					PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreatePendingPersonRequest(person2, _absence, period2, "Pending");

				persistPersonRequests(
					request1,
					request2
				);

				WithUnitOfWork.Do(() =>
				{
					((Person)PersonRepository.Get(person1.Id.Value)).SetDeleted();
				});

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target.GetPendingAndWaitlistedAbsenceRequests(period1, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(1);

					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest
							{PersonRequestId = request2.Id.Value, RequestStatus = PersonRequestStatus.Pending},
					}));
				});
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsRegardlessWorkflowControlSet()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("-", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});

				var person2 = createPerson("-", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});

				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
					PersonRepository.Add(person2);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(16));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person1, _absence, period3, "Pending");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person2, _absence, period2, "Waitlisted 2");

				persistPersonRequests(
					request1,
					request3,
					request2);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target.GetPendingAndWaitlistedAbsenceRequests(period3, _budgetGroup.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(3);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
					}));
				});
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsBasedOnBudgetGroups()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var budgetGroup1 = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };
				var budgetGroup2 = new BudgetGroup { Name = "group2", TimeZone = TimeZoneInfo.Utc };
				WithUnitOfWork.Do(() => {
					BudgetGroupRepository.Add(budgetGroup1);
					BudgetGroupRepository.Add(budgetGroup2);
				});

				var person1 = createPerson("-", _team, budgetGroup1, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				var person2 = createPerson("-", _team, budgetGroup2, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				WithUnitOfWork.Do(() =>
				{
					PersonRepository.Add(person1);
					PersonRepository.Add(person2);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(23));
				var request3 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period3, "Waitlisted 3");

				var period2 = new DateTimePeriod(_baseTime.AddHours(11), _baseTime.AddHours(20));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person2, _absence, period2, "Waitlisted 2");

				persistPersonRequests(
					request1,
					request3,
					request2);

				WithUnitOfWork.Do(() =>
				{
					var pendingAndWaitlistedRequests = Target.GetPendingAndWaitlistedAbsenceRequests(period1, budgetGroup1.Id.Value).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request1.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
					}));
				});
			}
		}

		[Test]
		public void ShouldReturnWaitlistedRequestsRegardlessBudgetGroupWhenAgentHasNoBudgetGroup()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				setUp();

				var person1 = createPerson("-", _team, _budgetGroup, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				var person2 = createPerson("-", _team, null, _waitlistedWcs, new List<DateOnly>
				{
					new DateOnly(2016, 03, 01),
					new DateOnly(2016, 03, 02)
				});
				WithUnitOfWork.Do(() => {
					PersonRepository.Add(person1);
					PersonRepository.Add(person2);
				});

				var period1 = new DateTimePeriod(_baseTime.AddHours(15), _baseTime.AddHours(19));
				var request1 = PersonRequestFactory.CreateWaitlistedPersonRequest(person1, _absence, period1, "Waitlisted 1");

				var period3 = new DateTimePeriod(_baseTime.AddHours(8), _baseTime.AddHours(16));
				var request3 = PersonRequestFactory.CreatePendingPersonRequest(person2, _absence, period3, "Pending");

				var period2 = new DateTimePeriod(_baseTime.AddHours(10), _baseTime.AddHours(18));
				var request2 = PersonRequestFactory.CreateWaitlistedPersonRequest(person2, _absence, period2, "Waitlisted 2");

				persistPersonRequests(
					request1,
					request3,
					request2
				);

				WithUnitOfWork.Do(() => {
					var pendingAndWaitlistedRequests = Target
					.GetPendingAndWaitlistedAbsenceRequests(period2, null).ToArray();

					pendingAndWaitlistedRequests.Length.Should().Be.EqualTo(2);
					Assert.True(sequenceEquals(pendingAndWaitlistedRequests, new List<PersonWaitlistedAbsenceRequest>
					{
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request3.Id.Value,RequestStatus = PersonRequestStatus.Pending},
						new PersonWaitlistedAbsenceRequest{PersonRequestId = request2.Id.Value,RequestStatus = PersonRequestStatus.Waitlisted}
					}));
				});
				
			}
		}

		private void setUp()
		{
			Now.Is(new DateTime(2016, 03, 02, 0, 0, 0, DateTimeKind.Utc));
			_defaultScenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
			_person = PersonFactory.CreatePerson("sdfoj");
			_absence = AbsenceFactory.CreateAbsence("Sick leave");

			_team = TeamFactory.CreateSimpleTeam("team");
			_site = SiteFactory.CreateSimpleSite("site");
			_team.Site = _site;
			_contract = new Contract("contract");
			_partTimePercentage = new PartTimePercentage("partTimePercentage");
			_contractSchedule = ContractScheduleFactory.CreateContractSchedule("contractSchedule");
			_waitlistedWcs = WorkflowControlSetFactory.CreateWorkFlowControlSetWithWaitlist(_absence,
						WaitlistProcessOrder.FirstComeFirstServed);
			_budgetGroup = new BudgetGroup { Name = "group1", TimeZone = TimeZoneInfo.Utc };

			WithUnitOfWork.Do(() =>
			{
				AbsenceRepository.Add(_absence);
				SiteRepository.Add(_site);
				TeamRepository.Add(_team);
				ContractRepository.Add(_contract);
				PartTimePercentageRepository.Add(_partTimePercentage);
				ContractScheduleRepository.Add(_contractSchedule);
				WorkflowControlSetRepository.Add(_waitlistedWcs);
				BudgetGroupRepository.Add(_budgetGroup);
			});
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

		private void persistPersonRequests(params IPersonRequest[] requests)
		{
			foreach (var request in requests)
			{
				WithUnitOfWork.Do(() =>
				{
					Target.Add(request);
				});
			}
		}

		private bool sequenceEquals(IList<PersonWaitlistedAbsenceRequest> actualList,
			IList<PersonWaitlistedAbsenceRequest> expectedList)
		{
			if (expectedList.Count != actualList.Count)
				return false;

			bool result = true;
			for (int i = 0; i < actualList.Count; i++)
			{
				result = result && assertEquals(expectedList[i], actualList[i]);
			}

			return result;
		}

		private bool assertEquals(PersonWaitlistedAbsenceRequest first, PersonWaitlistedAbsenceRequest second)
		{
			return first.PersonRequestId == second.PersonRequestId && first.RequestStatus == second.RequestStatus;
		}
	}
}