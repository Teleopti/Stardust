using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	class ApproveRequestCommandHandlerTest
	{
		private FakeCurrentScenario _scenario;
		private FakePersonRequestRepository _personRequestRepository;
		private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private FakeScheduleStorage _fakeScheduleStorage;
		private FakeScheduleDifferenceSaver _scheduleDifferenceSaver;
		private RequestApprovalServiceFactory _requestApprovalServiceFactory;
		private ApproveRequestCommandHandler _approveRequestCommandHandler;

		[SetUp]
		public void Setup()
		{
			_scenario = new FakeCurrentScenario();
			_personRequestRepository = new FakePersonRequestRepository();
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();

			_fakeScheduleStorage = new FakeScheduleStorage();
			_scheduleDifferenceSaver = new FakeScheduleDifferenceSaver(_fakeScheduleStorage);

			var businessRules = new BusinessRulesForPersonalAccountUpdate (_personAbsenceAccountRepository, new SchedulingResultStateHolder());

			_requestApprovalServiceFactory = new RequestApprovalServiceFactory(
				new SwapAndModifyService(null, null), 
				new FakeGlobalSettingDataRepository(),
				businessRules
			);
			
			_approveRequestCommandHandler = new ApproveRequestCommandHandler(
				_fakeScheduleStorage, _scheduleDifferenceSaver,
				new PersonRequestAuthorizationCheckerForTest(),
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				_personRequestRepository, _requestApprovalServiceFactory,
				_scenario
			);
		}

		[Test]
		public void ShouldUpdatePersonalAccountWhenRequestIsGranted()
		{
			var accountDay = new AccountDay(new DateOnly(2015, 12, 1))
			{
				BalanceIn = TimeSpan.FromDays(5),
				Accrued = TimeSpan.FromDays(20),
				Extra = TimeSpan.FromDays(0)
			};

			var absenceDateTimePeriod = new DateTimePeriod (2016, 01, 01, 00, 2016, 01, 01, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();

			createPersonAbsenceAccount(person, absence, accountDay);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift (_scenario.Current(), person, absenceDateTimePeriod); 
			
			_fakeScheduleStorage.Add(assignment);
			
			_approveRequestCommandHandler.Handle(new ApproveRequestCommand(){ PersonRequestId = personRequest.Id.Value });

			Assert.IsTrue(personRequest.IsApproved);
			Assert.AreEqual (24, accountDay.Remaining.TotalDays);
		}

		private PersonRequest createAbsenceRequest (IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var personRequestFactory = new PersonRequestFactory() {Person = person};
			var absenceRequest = personRequestFactory.CreateAbsenceRequest (absence,dateTimePeriod);
			var personRequest = absenceRequest.Parent as PersonRequest;
			personRequest.SetId (Guid.NewGuid());

			_personRequestRepository.Add (personRequest);

			return personRequest;
		}

		private void createPersonAbsenceAccount(IPerson person, IAbsence absence, IAccount accountDay)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			personAbsenceAccount.Add(accountDay);

			_personAbsenceAccountRepository.Add(personAbsenceAccount);
		}
	}
}
