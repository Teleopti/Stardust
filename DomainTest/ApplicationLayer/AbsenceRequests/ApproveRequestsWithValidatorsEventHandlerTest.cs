using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	public class ApproveRequestsWithValidatorsEventHandlerTest
	{
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IPersonRequestRepository _personRequestRepository;
		private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly FakeCurrentUnitOfWorkFactory _unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
		private FakeScenarioRepository _scenarioRepository;
		private readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
		private LoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private IPersonRepository _personRepository;
		private FakeScheduleDataReadScheduleStorage _scheduleRepository;
		private LoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private FakeBudgetDayRepository _fakeBudgetDayRepository;
		private FakeScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private PersonAccountUpdaterDummy _personAccountUpdaterDummy;
		private IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
		private readonly ISchedulingResultStateHolder _scheduleStateHolder = new FakeSchedulingResultStateHolder();
		private FakeMessageBrokerComposite _messageBroker;

		private IPerson _person;
		private IPersonRequest _personRequest;
		private ApproveRequestsWithValidatorsEvent _event;
		private ApproveRequestsWithValidatorsEventHandler _target;
		private IAbsence _absence;

		private DateTime _startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
		private DateTime _endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);

		[SetUp]
		public void SetUp()
		{
			var skillRepository = new FakeSkillRepository();
			var workloadRepository = new FakeWorkloadRepository();
			_currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			_personRepository = new FakePersonRepository();
			_writeProtectedScheduleCommandValidator = new WriteProtectedScheduleCommandValidator(
				new ConfigurablePermissions(), new FakeCommonAgentNameProvider(), new FakeLoggedOnUser(), new SwedishCulture());
			_personRequestRepository = new FakePersonRequestRepository();
			_scheduleProjectionReadOnlyPersister = new FakeScheduleProjectionReadOnlyPersister();
			_fakeBudgetDayRepository = new FakeBudgetDayRepository();
			var skillDayLoadHelper = new SkillDayLoadHelper(new FakeSkillDayRepository(),
				new MultisiteDayRepository(new FakeUnitOfWork()));
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_scheduleRepository = new FakeScheduleDataReadScheduleStorage();
			_loadSchedulesForRequestWithoutResourceCalculation =
				new LoadSchedulesForRequestWithoutResourceCalculation(_personAbsenceAccountRepository, _scheduleRepository);
			_scenarioRepository = new FakeScenarioRepository(_currentScenario.Current());

			_personAccountUpdaterDummy = new PersonAccountUpdaterDummy();
			_scheduleRepository = new FakeScheduleDataReadScheduleStorage();
			var peopleAndSkillLoaderDecider = new PeopleAndSkillLoaderDecider(_personRepository, null);
			_loadSchedulingStateHolderForResourceCalculation =
				new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository,
					skillRepository, workloadRepository, _scheduleRepository, peopleAndSkillLoaderDecider, skillDayLoadHelper);
			_messageBroker = new FakeMessageBrokerComposite();

			prepareTestData();

			_target = new ApproveRequestsWithValidatorsEventHandler(_currentUnitOfWorkFactory,
				getAbsenceRequestProcessor(_person, _personRequest), _personRequestRepository,
				_writeProtectedScheduleCommandValidator, _messageBroker);
		}

		private void prepareTestData()
		{
			_absence = AbsenceFactory.CreateAbsence("Holiday");
			var processAbsenceRequest = new GrantAbsenceRequest();
			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31),
				_absence, processAbsenceRequest, true);
			_person = createAndSetupPerson(_startDateTime, _endDateTime, workflowControlSet);
			_personRequest = createPendingAbsenceRequest(_person, _absence, new DateTimePeriod(_startDateTime, _endDateTime), true);

			_event = new ApproveRequestsWithValidatorsEvent
			{
				Validator = RequestValidatorsFlag.BudgetAllotmentValidator,
				PersonRequestIdList = new[] { _personRequest.Id.GetValueOrDefault() },
				TrackedCommandInfo = new TrackedCommandInfo { TrackId = new Guid() }
			};
		}

		[Test]
		public void ShouldApproveAbsenceRequestWithEnoughBudgetHeadCount()
		{
			setBudgetAndAllowance(_person, 1, 2);
			_target.Handle(_event);

			_personRequest.IsApproved.Should().Be.True();
			_messageBroker.SentCount().Should().Be(1);
		}

		[Test]
		public void ShouldDoNothingToAbsenceRequestWithNotEnoughBudgetHeadCount()
		{
			setBudgetAndAllowance(_person, 2, 1);
			_target.Handle(_event);

			_personRequest.IsApproved.Should().Be.False();
			_personRequest.IsPending.Should().Be.True();
			_messageBroker.SentCount().Should().Be(1);
		}

		[Test]
		public void ShouldApproveAbsenceRequestInWaitlistedStatus()
		{
			_personRequest = createPendingAbsenceRequest(_person, _absence, new DateTimePeriod(_startDateTime, _endDateTime), false);
			_personRequest.Deny(null, "test", new PersonRequestAuthorizationCheckerForTest(), true);
			_event.PersonRequestIdList = new Guid[] { _personRequest.Id.GetValueOrDefault() };
			setBudgetAndAllowance(_person, 1, 2);
			_target.Handle(_event);
			_personRequest.IsApproved.Should().Be.True();
			_messageBroker.SentCount().Should().Be(1);
		}

		private IAbsenceRequestProcessor getAbsenceRequestProcessor(IPerson person, IPersonRequest personRequest)
		{
			var period = personRequest.Request.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			_scheduleStateHolder.Schedules = new FakeScheduleStorage().FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { person }, new ScheduleDictionaryLoadOptions(true, true), period, _currentScenario.Current());
			return new AbsenceRequestProcessor(createAbsenceRequestUpdater(), () => _scheduleStateHolder);
		}

		private void setBudgetAndAllowance(IPerson person, int headCount, int allowance)
		{
			_scheduleProjectionReadOnlyPersister.SetNumberOfAbsencesPerDayAndBudgetGroup(headCount);
			var budgetGroup = new BudgetGroup();
			var startDate = new DateOnly(2000, 1, 1);
			var personContract = PersonContractFactory.CreatePersonContract();
			ITeam team = TeamFactory.CreateSimpleTeam();
			var personPeriod = new PersonPeriod(startDate, personContract, team)
			{
				BudgetGroup = budgetGroup
			};

			person.AddPersonPeriod(personPeriod);
			var budgetDay = new BudgetDay(budgetGroup, _currentScenario.Current(), new DateOnly(2016, 3, 1))
			{
				Allowance = allowance
			};
			_fakeBudgetDayRepository.Add(budgetDay);
		}

		private IAbsenceRequestUpdater createAbsenceRequestUpdater()
		{
			var resourceCalculator = new ResourceCalculationPrerequisitesLoader(_unitOfWorkFactory,
				new FakeContractScheduleRepository(),
				new FakeActivityRepository(), new FakeAbsenceRepository());
			var requestFactory =
				new RequestFactory(new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()),
					new PersonRequestAuthorizationCheckerForTest(), new FakeGlobalSettingDataRepository());
			var toggleManager = new FakeToggleManager();

			var budgetGroupHeadCountSpecification = new BudgetGroupHeadCountSpecification(_scenarioRepository,
				_fakeBudgetDayRepository, _scheduleProjectionReadOnlyPersister);
			var budgetGroupAllowanceSpecification = new BudgetGroupAllowanceSpecification(_currentScenario,
				_fakeBudgetDayRepository, _scheduleProjectionReadOnlyPersister);
			var absenceRequestStatusUpdater =
				new AbsenceRequestUpdater(new PersonAbsenceAccountProvider(_personAbsenceAccountRepository),
					resourceCalculator,
					new DefaultScenarioFromRepository(_scenarioRepository),
					_loadSchedulingStateHolderForResourceCalculation,
					_loadSchedulesForRequestWithoutResourceCalculation,
					requestFactory,
					new AlreadyAbsentSpecification(),
					new ScheduleIsInvalidSpecification(),
					new PersonRequestCheckAuthorization(),
					budgetGroupHeadCountSpecification,
					null,
					budgetGroupAllowanceSpecification,
					new FakeScheduleDifferenceSaver(_scheduleRepository),
					_personAccountUpdaterDummy, toggleManager);

			return absenceRequestStatusUpdater;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate,
			IAbsence absence, IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = waitlistingIsEnabled };
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = processAbsenceRequest,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}

		private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime,
			IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add(person);

			var assignmentOne = createAssignment(person, startDateTime, endDateTime, _currentScenario);
			_scheduleRepository.Set(new IScheduleData[] { assignmentOne });

			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate,
			ICurrentScenario currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				currentScenario.Current(), person, new DateTimePeriod(startDate, endDate));
		}

		private PersonRequest createPendingAbsenceRequest(IPerson person, IAbsence absence,
			DateTimePeriod requestDateTimePeriod, bool isPending)
		{
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod));

			personRequest.SetId(Guid.NewGuid());
			if (isPending)
			{
				personRequest.ForcePending();
			}
			_personRequestRepository.Add(personRequest);

			return personRequest;
		}
	}
}
