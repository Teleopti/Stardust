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
        private IAbsenceRequestProcessor _absenceRequestProcessor;
        private IPersonRequestRepository _personRequestRepository;
        private IAbsenceRequestUpdater _absenceRequestUpdater;
        FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
        readonly FakeCurrentUnitOfWorkFactory _unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
        private FakeScenarioRepository _scenarioRepository;
        readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
        private LoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
        private IPersonRepository _personRepository;
        FakeScheduleDataReadScheduleStorage _scheduleRepository;
        private LoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
        private FakeBudgetDayRepository _fakeBudgetDayRepository;
        private FakeScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
        private PersonAccountUpdaterDummy _personAccountUpdaterDummy;
        private IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
        private ISchedulingResultStateHolder _scheduleStateHolder = new FakeSchedulingResultStateHolder();

        [SetUp]
        public void SetUp()
        {
            var skillRepository = new FakeSkillRepository();
            var workloadRepository = new FakeWorkloadRepository();
            _currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
            _personRepository = new FakePersonRepository();
            _writeProtectedScheduleCommandValidator = new WriteProtectedScheduleCommandValidator(
                 new ConfigurablePermissions(), new FakeCommonAgentNameProvider(), new FakeLoggedOnUser(), new SwedishCulture());
            _personRequestRepository =new FakePersonRequestRepository();
            _scheduleProjectionReadOnlyPersister = new FakeScheduleProjectionReadOnlyPersister();
            _fakeBudgetDayRepository = new FakeBudgetDayRepository();
            var skillDayLoadHelper = new SkillDayLoadHelper(new FakeSkillDayRepository(),
                new MultisiteDayRepository(new FakeUnitOfWork()));
            _personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
            _scheduleRepository = new FakeScheduleDataReadScheduleStorage();
            _loadSchedulesForRequestWithoutResourceCalculation = new LoadSchedulesForRequestWithoutResourceCalculation(_personAbsenceAccountRepository, _scheduleRepository);
            _scenarioRepository = new FakeScenarioRepository(_currentScenario.Current());

            _personAccountUpdaterDummy = new PersonAccountUpdaterDummy();
            _scheduleRepository = new FakeScheduleDataReadScheduleStorage();
            var peopleAndSkillLoaderDecider = new PeopleAndSkillLoaderDecider(_personRepository, null);
            _loadSchedulingStateHolderForResourceCalculation = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository, skillRepository,
                workloadRepository, _scheduleRepository, peopleAndSkillLoaderDecider, skillDayLoadHelper);
        }

        [Test]
        public void ShouldApproveAbsenceRequestWithEnoughBudgetHeadCount()
        {
            var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
            var absence = AbsenceFactory.CreateAbsence("Holiday");
            var processAbsenceRequest= new GrantAbsenceRequest();
            var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, processAbsenceRequest, false);
            var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);
            var personRequest = createPendingAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));
            
            SetBudgetAndAllowance(person, 1, 2);

            _absenceRequestProcessor = GetAbsenceRequestProcessor(person, personRequest);
            var @event = new ApproveRequestsWithValidatorsEvent()
            {
                Validator = RequestValidatorsFlag.BudgetAllotmentValidator,
                PersonRequestIdList = new Guid[] { personRequest.Id.GetValueOrDefault() }
            };
            var handler = new ApproveRequestsWithValidatorsEventHandler(_currentUnitOfWorkFactory, _absenceRequestProcessor, _personRequestRepository, _writeProtectedScheduleCommandValidator);
            handler.Handle(@event);

            personRequest.IsApproved.Should().Be.True();
        }

        [Test]
        public void ShouldDoNothingToAbsenceRequestWithNotEnoughBudgetHeadCount()
        {
            var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
            var absence = AbsenceFactory.CreateAbsence("Holiday");
            var processAbsenceRequest = new GrantAbsenceRequest();
            var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, processAbsenceRequest, false);
            var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);
            var personRequest = createPendingAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));

            SetBudgetAndAllowance(person, 2, 1);

            _absenceRequestProcessor = GetAbsenceRequestProcessor(person, personRequest);
            var @event = new ApproveRequestsWithValidatorsEvent()
            {
                Validator = RequestValidatorsFlag.BudgetAllotmentValidator,
                PersonRequestIdList = new Guid[] { personRequest.Id.GetValueOrDefault() }
            };
            var handler = new ApproveRequestsWithValidatorsEventHandler(_currentUnitOfWorkFactory, _absenceRequestProcessor, _personRequestRepository, _writeProtectedScheduleCommandValidator);
            handler.Handle(@event);

            personRequest.IsApproved.Should().Be.False();
            personRequest.IsPending.Should().Be.True();
        }

        private IAbsenceRequestProcessor GetAbsenceRequestProcessor(IPerson person,IPersonRequest personRequest)
        {
            _scheduleStateHolder.Schedules = new FakeScheduleStorage().FindSchedulesForPersonsOnlyInGivenPeriod(new IPerson[] { person },
               new ScheduleDictionaryLoadOptions(true, true), personRequest.Request.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone())
               , _currentScenario.Current());
            return new AbsenceRequestProcessor(CreateAbsenceRequestUpdater(), () => _scheduleStateHolder);
        }
        private void SetBudgetAndAllowance(IPerson person,int headCount,int allowance)
        {
            _scheduleProjectionReadOnlyPersister.SetNumberOfAbsencesPerDayAndBudgetGroup(headCount);
            var budgetGroup = new BudgetGroup();
            DateOnly startDate = new DateOnly(2000, 1, 1);
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();
            ITeam team = TeamFactory.CreateSimpleTeam();
            var personPeriod = new PersonPeriod(startDate, personContract, team);
            personPeriod.BudgetGroup = budgetGroup;

            person.AddPersonPeriod(personPeriod);
            var budgetDay = new BudgetDay(budgetGroup, _currentScenario.Current(), new DateOnly(2016, 3, 1));
            budgetDay.Allowance = allowance;
            _fakeBudgetDayRepository.Add(budgetDay);
        }

        private IAbsenceRequestUpdater CreateAbsenceRequestUpdater()
        {
            var resourceCalculator = new ResourceCalculationPrerequisitesLoader(_unitOfWorkFactory,
                new FakeContractScheduleRepository(),
                new FakeActivityRepository(), new FakeAbsenceRepository());
            var requestFactory =
                new RequestFactory(new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()),
                    new PersonRequestAuthorizationCheckerForTest(), new FakeGlobalSettingDataRepository());
            var toggleManager = new FakeToggleManager();

            var absenceRequestStatusUpdater = new AbsenceRequestUpdater(new PersonAbsenceAccountProvider(_personAbsenceAccountRepository),
               resourceCalculator,
               new DefaultScenarioFromRepository(_scenarioRepository),
               _loadSchedulingStateHolderForResourceCalculation,
               _loadSchedulesForRequestWithoutResourceCalculation,
               requestFactory,
               new AlreadyAbsentSpecification(),
               new ScheduleIsInvalidSpecification(),
               new PersonRequestCheckAuthorization(),
               new BudgetGroupHeadCountSpecification(_scenarioRepository, _fakeBudgetDayRepository,
                   _scheduleProjectionReadOnlyPersister),
               null,
               new BudgetGroupAllowanceSpecification(_currentScenario, _fakeBudgetDayRepository,
                   _scheduleProjectionReadOnlyPersister),
               new FakeScheduleDifferenceSaver(_scheduleRepository),
               _personAccountUpdaterDummy, toggleManager);

            return absenceRequestStatusUpdater;
        }
        private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence, IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
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
        private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime, IWorkflowControlSet workflowControlSet)
        {
            var person = PersonFactory.CreatePersonWithId();
            _personRepository.Add(person);

            var assignmentOne = createAssignment(person, startDateTime, endDateTime, _currentScenario);
            _scheduleRepository.Set(new IScheduleData[] { assignmentOne });

            person.WorkflowControlSet = workflowControlSet;

            return person;
        }
        private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
        {
            return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
                currentScenario.Current(),
                person,
                new DateTimePeriod(startDate, endDate));
        }

        private PersonRequest createPendingAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
        {
            var personRequest = new PersonRequest(person, new Domain.AgentInfo.Requests.AbsenceRequest(absence, requestDateTimePeriod));

            personRequest.SetId(Guid.NewGuid());
            personRequest.ForcePending();
            _personRequestRepository.Add(personRequest);

            return personRequest;
        }

    }
}
