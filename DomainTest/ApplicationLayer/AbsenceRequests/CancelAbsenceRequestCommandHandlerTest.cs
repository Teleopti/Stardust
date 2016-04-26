using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	public class CancelAbsenceRequestCommandHandlerTest
	{
		private FakePersonRequestRepository _requestRepository;
		private FakeCurrentScenario _scenario;
		private FakePersonAbsenceRepository _personAbsenceRepository;
		private PersonRequestAuthorizationCheckerForTest _personRequestAuthorizationChecker;
		private PersonAbsenceRemover _personAbsenceRemover;
		private FakeScheduleStorage _scheduleStorage;
		private BusinessRulesForPersonalAccountUpdate _businessRulesForAccountUpdate;
		private SaveSchedulePartService _saveSchedulePartService;
		private PersonAbsenceCreator _personAbsenceCreator;
		private FakeCommonAgentNameProvider _fakeCommonAgentNameProvider;
		private FakeLoggedOnUser _loggedOnUser;
		private SwedishCulture _culture;
		private IAbsence _absence;
		private IPerson _person;
		private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private FakeSchedulingResultStateHolder _schedulingResultStateHolder;

		[SetUp]
		public void Setup()
		{
			_scenario = new FakeCurrentScenario();
			_requestRepository = new FakePersonRequestRepository();
			_personAbsenceRepository = new FakePersonAbsenceRepository();
			_personRequestAuthorizationChecker = new PersonRequestAuthorizationCheckerForTest();
			_schedulingResultStateHolder = new FakeSchedulingResultStateHolder();
			_fakeCommonAgentNameProvider = new FakeCommonAgentNameProvider();
			_loggedOnUser = new FakeLoggedOnUser();
			_culture = new SwedishCulture();

			_absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			_person = PersonFactory.CreatePerson("Yngwie", "Malmsteen").WithId();

			_scheduleStorage = new FakeScheduleStorage();

			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_businessRulesForAccountUpdate = new BusinessRulesForPersonalAccountUpdate(_personAbsenceAccountRepository, _schedulingResultStateHolder);

			var scheduleDifferenceSaver = new FakeScheduleDifferenceSaver(_scheduleStorage);
			_saveSchedulePartService = new SaveSchedulePartService(scheduleDifferenceSaver, _personAbsenceAccountRepository);
			_personAbsenceCreator = new PersonAbsenceCreator(_saveSchedulePartService, _businessRulesForAccountUpdate);
			_personAbsenceRemover = new PersonAbsenceRemover(_businessRulesForAccountUpdate, _saveSchedulePartService, _personAbsenceCreator, _loggedOnUser, _personRequestAuthorizationChecker);

		}

		[Test]
		public void ShouldCancelAcceptedRequestAndDeleteRelatedAbsence()
		{
			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			var personRequest = basicCancelAbsenceRequest(cancelRequestCommand, _personRequestAuthorizationChecker);

			Assert.AreEqual (true, personRequest.IsCancelled);
			Assert.IsTrue(_scheduleStorage.LoadAll().IsEmpty());
			Assert.IsTrue (cancelRequestCommand.ErrorMessages.IsEmpty() );
		}
		
		[Test]
		public void ShouldNotCancelRequestWhenNoPermission()
		{
			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			var personRequestAuthorizationChecker = new personRequestAuthorizationCheckerConfigurable()
			{
				HasCancelPermission = false
			};
			
			var personRequest = basicCancelAbsenceRequest(cancelRequestCommand, personRequestAuthorizationChecker);
			
			Assert.AreEqual(false, personRequest.IsCancelled);
			Assert.AreEqual(1, _scheduleStorage.LoadAll().Count);
			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
		}

		[Test]
		public void ShouldFailGracefullyWhenAttemptingToCancelRequestWhereAbsenceCannotBeFound()
		{
			
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 01);

			var absenceRequest = createApprovedAbsenceRequest(_absence, dateTimePeriodOfAbsenceRequest, _person);
			var personRequest = absenceRequest.Parent as PersonRequest;

			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			new CancelAbsenceRequestCommandHandler(_requestRepository, _personRequestAuthorizationChecker, _personAbsenceRepository, _personAbsenceRemover,
				_loggedOnUser, _culture, _fakeCommonAgentNameProvider, _scheduleStorage, _scenario)
				.Handle(cancelRequestCommand);

			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
			Assert.AreEqual(string.Format(Resources.CouldNotCancelRequestNoAbsence,
				_fakeCommonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(_person),
				absenceRequest.Period.StartDateTimeLocal(_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).Date.ToString("d", _culture.GetCulture())),
				cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		public void ShouldFailGracefullyWhenAttemptingToCancelRequestWhereAbsenceRequestHasNotBeenAccepted()
		{
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 01);
			
			var absenceRequest = new AbsenceRequest(_absence, dateTimePeriodOfAbsenceRequest);
			var personRequest = new PersonRequest (_person, absenceRequest).WithId();
			_requestRepository.Add(personRequest);
			personRequest.Pending();
			
			var cancelRequestCommand = new CancelAbsenceRequestCommand()
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault()
			};

			new CancelAbsenceRequestCommandHandler(_requestRepository, _personRequestAuthorizationChecker, _personAbsenceRepository, _personAbsenceRemover,
				_loggedOnUser, _culture, _fakeCommonAgentNameProvider, _scheduleStorage, _scenario)
				.Handle(cancelRequestCommand);

			Assert.AreEqual(1, cancelRequestCommand.ErrorMessages.Count);
			Assert.AreEqual(string.Format(Resources.CanOnlyCancelApprovedAbsenceRequest,
				_fakeCommonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(_person),
				absenceRequest.Period.StartDateTimeLocal(_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone()).Date.ToString("d", _culture.GetCulture())),
				cancelRequestCommand.ErrorMessages[0]);
		}

		[Test]
		public void CancellingARequestWithMultipleAbsencesShouldUpdatePersonAccount ()
		{
			var accountDay = new AccountDay(new DateOnly(2016, 03, 1))
			{
				BalanceIn = TimeSpan.FromDays(5),
				Accrued = TimeSpan.FromDays(25),
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = TimeSpan.FromDays(8)  // have used 8 days
			};

			createPersonAbsenceAccount (_person, _absence, accountDay);

			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			cancelAbsenceRequestWithMultipleAbsences(cancelRequestCommand, true);

			Assert.AreEqual(30, accountDay.Remaining.TotalDays);
		}


		[Test] 
		public void ShouldCancelAcceptedRequestAndDeleteMultipleRelatedAbsences()
		{
			var cancelRequestCommand = new CancelAbsenceRequestCommand();
			var personRequest = cancelAbsenceRequestWithMultipleAbsences(cancelRequestCommand);

			Assert.IsTrue(personRequest.IsCancelled);
			Assert.IsTrue(_scheduleStorage.LoadAll().IsEmpty());
			Assert.IsTrue(cancelRequestCommand.ErrorMessages.IsEmpty());
		}

		private PersonRequest cancelAbsenceRequestWithMultipleAbsences (CancelAbsenceRequestCommand cancelRequestCommand, bool checkPersonAccounts = false)
		{
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 14);
			var absenceRequest = createApprovedAbsenceRequest (_absence, dateTimePeriodOfAbsenceRequest, _person);
			var personRequest = absenceRequest.Parent as PersonRequest;

			if (checkPersonAccounts)
			{
				createShiftsForPeriod(dateTimePeriodOfAbsenceRequest);
			}
			
			createPersonAbsence (_absence, new DateTimePeriod (2016, 03, 05, 2016, 03, 07), _person, absenceRequest);
			createPersonAbsence (_absence, new DateTimePeriod (2016, 03, 09, 2016, 03, 13), _person, absenceRequest);
			createPersonAbsence (_absence, new DateTimePeriod (2016, 03, 01, 2016, 03, 03), _person, absenceRequest);

			cancelRequestCommand.PersonRequestId = personRequest.Id.GetValueOrDefault();

			new CancelAbsenceRequestCommandHandler (_requestRepository, _personRequestAuthorizationChecker,
				_personAbsenceRepository, _personAbsenceRemover,
				_loggedOnUser, _culture, _fakeCommonAgentNameProvider, _scheduleStorage, _scenario)
				.Handle (cancelRequestCommand);

			return personRequest;
		}


		private void createPersonAbsenceAccount(IPerson person, IAbsence absence, IAccount accountDay)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			personAbsenceAccount.Add(accountDay);

			_personAbsenceAccountRepository.Add(personAbsenceAccount);

	
		}

		private PersonRequest basicCancelAbsenceRequest(CancelAbsenceRequestCommand cancelRequestCommand, IPersonRequestCheckAuthorization personRequestAuthorizationChecker)
		{
			var dateTimePeriodOfAbsenceRequest = new DateTimePeriod(2016, 03, 01, 2016, 03, 03);

			var absenceRequest = createApprovedAbsenceRequest(_absence, dateTimePeriodOfAbsenceRequest, _person);
			var personRequest = absenceRequest.Parent as PersonRequest;

			createPersonAbsence(_absence, dateTimePeriodOfAbsenceRequest, _person, absenceRequest);

			cancelRequestCommand.PersonRequestId = personRequest.Id.GetValueOrDefault();

			new CancelAbsenceRequestCommandHandler(
				_requestRepository, personRequestAuthorizationChecker, _personAbsenceRepository, _personAbsenceRemover,
				new FakeLoggedOnUser(), new SwedishCulture(), new FakeCommonAgentNameProvider(), _scheduleStorage, _scenario)
				.Handle(cancelRequestCommand);
			return personRequest;
		}

		private AbsenceRequest createApprovedAbsenceRequest (IAbsence absence, DateTimePeriod dateTimePeriodOfAbsenceRequest,IPerson person)
		{
			var absenceRequest = new AbsenceRequest (absence, dateTimePeriodOfAbsenceRequest);
			createApprovedPersonRequest (person, absenceRequest);
			return absenceRequest;
		}
		
		private PersonRequest createApprovedPersonRequest (IPerson person, AbsenceRequest absenceRequest)
		{
			var personRequest = new PersonRequest (person, absenceRequest).WithId();
			_requestRepository.Add (personRequest);

			personRequest.Pending();
			personRequest.Approve (new ApprovalServiceForTest(), _personRequestAuthorizationChecker);
			return personRequest;
		}

		private PersonAbsence createPersonAbsence (IAbsence absence, DateTimePeriod dateTimePeriodOfAbsenceRequest, IPerson person, AbsenceRequest absenceRequest)
		{
			var absenceLayer = new AbsenceLayer (absence, dateTimePeriodOfAbsenceRequest);
			var personAbsence = new PersonAbsence (person, _scenario.Current(), absenceLayer, absenceRequest).WithId();

			_personAbsenceRepository.Add (personAbsence);
			_scheduleStorage.Add (personAbsence);

			return personAbsence;
		}



		private void createShiftsForPeriod(DateTimePeriod period)
		{
			foreach (var day in period.WholeDayCollection(TimeZoneInfo.Utc))
			{
				// need to have a shift otherwise personAccount day off will not be affected
				_scheduleStorage.Add(createAssignment(_person, day.StartDateTime, day.EndDateTime, _scenario));
			}
		}

		private static IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));
		}

		private class personRequestAuthorizationCheckerConfigurable : IPersonRequestCheckAuthorization
		{
			public bool HasEditPermission { get; set; }
			public bool HasViewPermission { get; set; }
			public bool HasCancelPermission { get; set; }

			public personRequestAuthorizationCheckerConfigurable()
			{
				HasEditPermission = HasViewPermission = HasCancelPermission = true;
			}
			
			public void VerifyEditRequestPermission(IPersonRequest personRequest)
			{
			}

			public bool HasEditRequestPermission(IPersonRequest personRequest)
			{
				return HasEditPermission;
			}

			public bool HasViewRequestPermission(IPersonRequest personRequest)
			{
				return HasViewPermission;
			}

			public bool HasCancelRequestPermission(IPersonRequest personRequest)
			{
				return HasCancelPermission;
			}
		}
	}

}
