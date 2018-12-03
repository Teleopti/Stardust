using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
	//copied from SchedulerStateHolderTest
	public class SchedulingScreenStateTest
	{
		private SchedulerStateHolder target;
		private ScheduleDateTimePeriod dtp;
		private MockRepository mocks;
		private IScenario scenario;
		private IList<IPerson> selectedPersons;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private Guid _guid1;
		private Guid _guid2;
		private Guid _guid3;
		private IPersistableScheduleDataPermissionChecker _permissionChecker;
		private IDisposable auth;
		
		[SetUp]
		public void Setup()
		{
			var period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
			dtp = new ScheduleDateTimePeriod(period);
			scenario = ScenarioFactory.CreateScenarioAggregate("test", true);
			_person1 = PersonFactory.CreatePerson("first", "last");
			_person2 = PersonFactory.CreatePerson("firstName", "lastName");
			_person3 = PersonFactory.CreatePerson("firstName", "lastName");
			_guid1 = Guid.NewGuid();
			_guid2 = Guid.NewGuid();
			_guid3 = Guid.NewGuid();
			_person1.SetId(_guid1);
			_person2.SetId(_guid2);
			_person3.SetId(_guid3);
			selectedPersons = new List<IPerson>{_person1, _person2};
			var schedulingResultStateHolder = SchedulingResultStateHolderFactory.Create(period);
			target = new SchedulerStateHolder(scenario,
				new DateOnlyPeriodAsDateTimePeriod(
					dtp.VisiblePeriod.ToDateOnlyPeriod(TimeZoneInfoFactory.UtcTimeZoneInfo()),
					TimeZoneInfoFactory.UtcTimeZoneInfo()), selectedPersons, new DisableDeletedFilter(new CurrentUnitOfWork(new FakeCurrentUnitOfWorkFactory(null))), schedulingResultStateHolder, new TimeZoneGuard());
			target.SetRequestedScenario(scenario);
			mocks = new MockRepository();
			_permissionChecker = new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make());
			auth = CurrentAuthorization.ThreadlyUse(new FullPermission());
		}
		
		[TearDown]
		public void Teardown()
		{
			auth?.Dispose();
		}
		
		[Test]
		public void LoadPersonRequests_ShiftTradeAfterLoadedPeriodAndReferred_ShouldNotLoad()
		{
			var unitOfWork = MockRepository.GenerateStrictMock<IUnitOfWork>();
			var repositoryFactory = MockRepository.GenerateStrictMock<IRepositoryFactory>();
			var personRequestRepository = MockRepository.GenerateStrictMock<IPersonRequestRepository>();
			var scheduleRepository = MockRepository.GenerateStrictMock<IFindSchedulesForPersons>();
			var person = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> {person};
			var dateOnly = new DateOnly(2001, 12, 31);
			var personRequest = new PersonRequest(person, new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(person, person2, dateOnly, dateOnly)
			}));
			var requestList = new List<IPersonRequest> { personRequest };
			personRequest.ForcePending();
			((IShiftTradeRequest) personRequest.Request).SetShiftTradeStatus(ShiftTradeStatus.Referred, new PersonRequestAuthorizationCheckerForTest());

			scheduleRepository.Expect(s => s.FindSchedulesForPersons(null, null, null, new DateTimePeriod(), personList, false))
				.IgnoreArguments()
				.Return(new ScheduleDictionary(scenario, dtp, _permissionChecker, CurrentAuthorization.Make()));
			target.LoadSchedules(scheduleRepository, null, null, dtp.VisiblePeriod);

			repositoryFactory.Expect(r => r.CreatePersonRequestRepository(unitOfWork)).Return(personRequestRepository);
			personRequestRepository.Expect(
					p => p.FindAllRequestModifiedWithinPeriodOrPending(personList, new DateTimePeriod(2001, 1, 1, 2001, 1, 2)))
				.Return(requestList)
				.IgnoreArguments();

			var innerTarget = new SchedulingScreenState(null, target);

			innerTarget.LoadPersonRequests(unitOfWork, repositoryFactory, new PersonRequestAuthorizationCheckerForTest(), 10);

			innerTarget.PersonRequests.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void LoadPersonRequests_ShiftTradeWithinLoadedPeriodAndReferred_ShouldNotLoad()
		{
			var unitOfWork = MockRepository.GenerateStrictMock<IUnitOfWork>();
			var repositoryFactory = MockRepository.GenerateStrictMock<IRepositoryFactory>();
			var personRequestRepository = MockRepository.GenerateStrictMock<IPersonRequestRepository>();
			var scheduleRepository = MockRepository.GenerateStrictMock<IFindSchedulesForPersons>();
			var person = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person };
			var dateOnly = new DateOnly(2000, 10, 1);
			var personRequest = new PersonRequest(person, new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(person, person2, dateOnly, dateOnly)
			}));
			var requestList = new List<IPersonRequest> { personRequest };
			personRequest.ForcePending();
			((IShiftTradeRequest)personRequest.Request).SetShiftTradeStatus(ShiftTradeStatus.Referred, new PersonRequestAuthorizationCheckerForTest());

			scheduleRepository.Expect(s => s.FindSchedulesForPersons(null, null, null, new DateTimePeriod(), personList, false))
				.IgnoreArguments()
				.Return(new ScheduleDictionary(scenario, dtp, _permissionChecker, CurrentAuthorization.Make()));
			target.LoadSchedules(scheduleRepository, null, null, dtp.VisiblePeriod);

			repositoryFactory.Expect(r => r.CreatePersonRequestRepository(unitOfWork)).Return(personRequestRepository);
			personRequestRepository.Expect(
					p => p.FindAllRequestModifiedWithinPeriodOrPending(personList, new DateTimePeriod(2000, 1, 1, 2002, 1, 1)))
				.Return(requestList)
				.IgnoreArguments();

			var innerTarget = new SchedulingScreenState(null, target);
			innerTarget.LoadPersonRequests(unitOfWork, repositoryFactory, new PersonRequestAuthorizationCheckerForTest(), 10);

			innerTarget.PersonRequests.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void LoadPersonRequests_CheckLoadPeriod()
		{
			var unitOfWork = MockRepository.GenerateStrictMock<IUnitOfWork>();
			var repositoryFactory = MockRepository.GenerateStrictMock<IRepositoryFactory>();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			repositoryFactory.Expect(r => r.CreatePersonRequestRepository(unitOfWork))
				.Return(personRequestRepository);

			const int passedNumberOfDays = 12;
			var expectedPeriod = new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-passedNumberOfDays),
				DateTime.SpecifyKind(DateTime.MaxValue.Date, DateTimeKind.Utc));

			personRequestRepository
				.Expect(x =>
					x.FindAllRequestModifiedWithinPeriodOrPending((IList<IPerson>) null, new DateTimePeriod()))
				.IgnoreArguments().Return(new List<IPersonRequest>());

			var innerTarget = new SchedulingScreenState(null, target);
			innerTarget.LoadPersonRequests(unitOfWork, repositoryFactory, new PersonRequestAuthorizationCheckerForTest(),
				passedNumberOfDays);

			personRequestRepository.AssertWasCalled(x =>
				x.FindAllRequestModifiedWithinPeriodOrPending(Arg<IList<IPerson>>.Is.Anything,
					Arg<DateTimePeriod>.Is.Equal(expectedPeriod)));
		}

		[Test]
		public void LoadPersonRequests_ShiftTradeAfterLoadedPeriodAndOkByMe_ShouldNotLoad()
		{
			var unitOfWork = MockRepository.GenerateStrictMock<IUnitOfWork>();
			var repositoryFactory = MockRepository.GenerateStrictMock<IRepositoryFactory>();
			var personRequestRepository = MockRepository.GenerateStrictMock<IPersonRequestRepository>();
			var scheduleRepository = MockRepository.GenerateStrictMock<IFindSchedulesForPersons>();
			var person = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var personList = new List<IPerson> { person };
			var dateOnly = new DateOnly(2001, 12, 31);
			var personRequest = new PersonRequest(person, new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				new ShiftTradeSwapDetail(person, person2, dateOnly, dateOnly)
			}));
			var requestList = new List<IPersonRequest> { personRequest };
			personRequest.ForcePending();
			((IShiftTradeRequest)personRequest.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByMe, new PersonRequestAuthorizationCheckerForTest());

			scheduleRepository.Expect(s => s.FindSchedulesForPersons(null, null, null, new DateTimePeriod(), personList, false))
				.IgnoreArguments()
				.Return(new ScheduleDictionary(scenario, dtp, _permissionChecker, CurrentAuthorization.Make()));
			target.LoadSchedules(scheduleRepository, null, null, dtp.VisiblePeriod);

			repositoryFactory.Expect(r => r.CreatePersonRequestRepository(unitOfWork)).Return(personRequestRepository);
			personRequestRepository.Expect(
					p => p.FindAllRequestModifiedWithinPeriodOrPending(personList, new DateTimePeriod(2001, 1, 1, 2001, 1, 2)))
				.Return(requestList)
				.IgnoreArguments();

			var innerTarget = new SchedulingScreenState(null, target);
			innerTarget.LoadPersonRequests(unitOfWork, repositoryFactory, new PersonRequestAuthorizationCheckerForTest(), 10);

			innerTarget.PersonRequests.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void VerifyRequestUpdateFromBroker()
		{
			IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
			var scheduleRepository = mocks.StrictMock<IScheduleStorage>();
			IPerson person1 = PersonFactory.CreatePerson();

			IPersonRequest personRequest = new PersonRequest(person1);
			personRequest.SetId(Guid.NewGuid());

			IList<IPersonRequest> originalList = new List<IPersonRequest> {personRequest};
			target.SchedulingResultState.LoadedAgents = new Collection<IPerson> {person1};
			var innerTarget = new SchedulingScreenState(null, target);
			using (mocks.Record())
			{
				Expect.Call(personRequestRepository.Find(personRequest.Id.Value))
					.Return(originalList[0]);
			}
			using (mocks.Playback())
			{
				personRequest.Changed = true;
				IPersonRequest updatedRequest = innerTarget.RequestUpdateFromBroker(personRequestRepository, personRequest.Id.Value, scheduleRepository);
				Assert.AreSame(personRequest, updatedRequest);
			}
			Assert.IsFalse(personRequest.Changed);
			Assert.AreEqual(1, innerTarget.PersonRequests.Count);
		}

		[Test]
		public void ShouldFilterOvertimeRequestWhenUpdateFromBroker()
		{
			IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
			var scheduleRepository = mocks.StrictMock<IScheduleStorage>();
			IPerson person1 = PersonFactory.CreatePerson();
			IRequest overtimeRequest =
				new OvertimeRequest(new MultiplicatorDefinitionSet("-", MultiplicatorType.Overtime),
					new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(2)));
			IPersonRequest personRequest = new PersonRequest(person1, overtimeRequest);
			personRequest.SetId(Guid.NewGuid());

			IList<IPersonRequest> originalList = new List<IPersonRequest> { personRequest };
			target.SchedulingResultState.LoadedAgents = new Collection<IPerson> { person1 };
			var innerTarget = new SchedulingScreenState(null, target);
			using (mocks.Record())
			{
				Expect.Call(personRequestRepository.Find(personRequest.Id.Value))
					.Return(originalList[0]);
			}
			using (mocks.Playback())
			{
				IPersonRequest updatedRequest = innerTarget.RequestUpdateFromBroker(personRequestRepository, personRequest.Id.Value, scheduleRepository);
				Assert.IsNull(updatedRequest);
			}
			Assert.AreEqual(0, innerTarget.PersonRequests.Count);
		}

		[Test]
		public void VerifyRequestUpdateFromBrokerIfNotPresent()
		{
			IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
			var scheduleRepository = mocks.StrictMock<IScheduleStorage>();
			IPerson person1 = PersonFactory.CreatePerson();

			IPersonRequest personRequest = new PersonRequest(person1);
			personRequest.SetId(Guid.NewGuid());
			target.SchedulingResultState.LoadedAgents = new Collection<IPerson> { person1 };
			var innerTarget = new SchedulingScreenState(null, target);
			using (mocks.Record())
			{
				Expect.Call(personRequestRepository.Find(personRequest.Id.Value))
					.Return(null);
			}
			using (mocks.Playback())
			{
				personRequest.Changed = true;
				IPersonRequest updatedRequest = innerTarget.RequestUpdateFromBroker(personRequestRepository, personRequest.Id.Value, scheduleRepository);
				Assert.IsNull(updatedRequest);
			}
			Assert.IsTrue(personRequest.Changed);
			Assert.AreEqual(0, innerTarget.PersonRequests.Count);
		}

		[Test]
		public void VerifyRequestDeleteFromBroker()
		{
			IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
			IRepositoryFactory repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
			IPersonRequestRepository personRequestRepository = mocks.StrictMock<IPersonRequestRepository>();
			IPerson person1 = mocks.StrictMock<IPerson>();

			IPersonRequest personRequest = new PersonRequest(person1);
			personRequest.SetId(Guid.NewGuid());

			IList<IPersonRequest> originalList = new List<IPersonRequest> { personRequest };
			IList<IPerson> personList = new List<IPerson> { person1 };
			target.SetRequestedScenario(ScenarioFactory.CreateScenarioAggregate("test", true));
			var innerTarget = new SchedulingScreenState(null, target);
			using (mocks.Record())
			{
				Expect.Call(repositoryFactory.CreatePersonRequestRepository(unitOfWork)).Return(personRequestRepository);
				Expect.Call(personRequestRepository.FindAllRequestModifiedWithinPeriodOrPending(personList, new DateTimePeriod(2001, 1, 1, 2001, 1, 2))).Return(originalList).IgnoreArguments();
			   
			}
			using (mocks.Playback())
			{
				innerTarget.LoadPersonRequests(unitOfWork, repositoryFactory, new PersonRequestAuthorizationCheckerForTest(), 10);
				IPersonRequest deletedRequest = innerTarget.RequestDeleteFromBroker(personRequest.Id.Value);
				Assert.AreSame(personRequest, deletedRequest);
			}
			Assert.AreEqual(0,innerTarget.PersonRequests.Count);
		}

		[Test]
		public void VerifyChangedRequests()
		{
			var innerTarget = new SchedulingScreenState(null, target);
			innerTarget.PersonRequests.Add(new PersonRequest(PersonFactory.CreatePerson()));
			Assert.IsFalse(innerTarget.ChangedRequests());
			
			innerTarget.PersonRequests[0].Changed = true;
			Assert.IsTrue(innerTarget.ChangedRequests());
		}
	}
}