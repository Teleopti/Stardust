using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.AbsenceReport;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{

	[TestFixture]
	class NewAbsenceReportConsumerNoMocksTest
	{
		readonly FakeCurrentUnitOfWorkFactory _unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
		readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
		private FakeSchedulingResultStateHolder _schedulingResultStateHolder;
		private FakeScheduleDataReadScheduleStorage _scheduleRepository;
		private UpdateScheduleProjectionReadModel _scheduleProjectionReadModel;
		private FakeScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private LoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private IPersonRepository _personRepository;

		[SetUp]
		public void SetUp()
		{
			_schedulingResultStateHolder = new FakeSchedulingResultStateHolder();
			_scheduleRepository = new FakeScheduleDataReadScheduleStorage();
			_scheduleProjectionReadOnlyRepository = new FakeScheduleProjectionReadOnlyRepository();
			_scheduleProjectionReadModel = new UpdateScheduleProjectionReadModel(new ProjectionChangedEventBuilder(), _scheduleProjectionReadOnlyRepository);
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_loadSchedulesForRequestWithoutResourceCalculation = new LoadSchedulesForRequestWithoutResourceCalculation(_schedulingResultStateHolder, _personAbsenceAccountRepository, _scheduleRepository);
			_personRepository = new FakePersonRepository();
		}
		
		[Test]
		public void VerifyAbsenceIsCreatedForAbsenceReport()
		{
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var startDate = new DateTime(2016, 02, 17, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 02, 19, 23, 59, 0, DateTimeKind.Utc);

			var person = setupPerson(startDate, endDate, absence);

			var absenceReportConsumer = setupAbsenceReportConsumer();

			var absenceReport = new NewAbsenceReportCreated()
			{
				RequestedDate = startDate,
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault()
			};

			absenceReportConsumer.Consume(absenceReport);

			var scheduleLoadOptions = new ScheduleDictionaryLoadOptions (false, false);
			var schedules = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod (person, scheduleLoadOptions, new DateTimePeriod (startDate, startDate.AddDays (1)), _currentScenario.Current());
			var scheduleDay = schedules.SchedulesForDay (new DateOnly (startDate)).FirstOrDefault();
			var personAbsence = scheduleDay.PersonAbsenceCollection().SingleOrDefault(abs => abs.Layer.Payload == absence && abs.Person == person);

			Assert.IsNotNull(personAbsence);
		}


		[Test]
		public void VerifyPersonAccountIsUpdated()
		{
			var absence = AbsenceFactory.CreateAbsenceWithId();
			
			var startDate = new DateTime(2016, 02, 17, 0, 0, 0, DateTimeKind.Utc);
			var endDate = new DateTime(2016, 02, 19, 23, 59, 0, DateTimeKind.Utc);

			var person = setupPerson(startDate, endDate, absence);
			var absenceReportConsumer = setupAbsenceReportConsumer();

			var absenceReport = new NewAbsenceReportCreated()
			{
				RequestedDate = startDate,
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault()
			};

			var accountDay = new AccountDay(new DateOnly(2015, 12, 1))
			{
				BalanceIn = TimeSpan.FromDays(5),
				Accrued = TimeSpan.FromDays(20),
				Extra = TimeSpan.FromDays(0)
			};

			createPersonAbsenceAccount(person, absence, accountDay);

			absenceReportConsumer.Consume(absenceReport);

			Assert.AreEqual(24, accountDay.Remaining.TotalDays);
		}

		private NewAbsenceReportConsumer setupAbsenceReportConsumer()
		{
			var requestFactory =
				new RequestFactory (new SwapAndModifyService (new SwapService(), new DoNothingScheduleDayChangeCallBack()),
					new PersonRequestAuthorizationCheckerForTest(), _schedulingResultStateHolder, new FakeGlobalSettingDataRepository());

			var scheduleDictionarySaver = new FakeScheduleDifferenceSaver (_scheduleRepository);

			var businessRules = new BusinessRulesForPersonalAccountUpdate (_personAbsenceAccountRepository,
				_schedulingResultStateHolder);

			var absenceReportConsumer = new NewAbsenceReportConsumer (_unitOfWorkFactory, _currentScenario,
				_schedulingResultStateHolder, requestFactory, scheduleDictionarySaver, _scheduleProjectionReadModel,
				_loadSchedulesForRequestWithoutResourceCalculation, _personRepository, businessRules);
			return absenceReportConsumer;
		}

		private IPerson setupPerson (DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add (person);

			person.WorkflowControlSet = createWorkFlowControlSet (startDate, endDate, absence);

			var assignmentOne = createAssignment (person, startDate, startDate.AddHours (8), _currentScenario);
			_scheduleRepository.Set (new IScheduleData[] {assignmentOne});
			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet();
			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);
			workflowControlSet.AllowedAbsencesForReport = new[] { absence };

			return workflowControlSet;
		}

		private static IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));
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
