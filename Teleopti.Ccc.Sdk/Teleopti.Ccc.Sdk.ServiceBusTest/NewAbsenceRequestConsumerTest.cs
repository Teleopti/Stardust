﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture]
    public class NewAbsenceRequestConsumerTest
    {
        private NewAbsenceRequestConsumer _target;
        private MockRepository _mockRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IUnitOfWork _unitOfWork;
        private IPersonRequestRepository _personRequestRepository;
        private IPersonRequest _personRequest;
        private IAbsenceRequest _absenceRequest;
        private IPerson _person;
        private readonly NewAbsenceRequestCreated _message = new NewAbsenceRequestCreated { PersonRequestId = Guid.NewGuid() };
        private readonly IScenario _scenario = new Scenario("Test");
        private readonly IAbsence _absence = new Absence {Description = new Description("Vacation", "VAC"), Tracker = Tracker.CreateDayTracker()};
        private readonly DateTimePeriod _period = new DateTimePeriod(2010,3,30,2010,3,31);
        private IWorkflowControlSet _workflowControlSet;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IAbsenceRequestOpenPeriodMerger _merger;
        private IScheduleDictionary _scheduleDictionary;
        private IPersonAbsenceAccountProvider _personAbsenceAccountProvider;
        private IScheduleRepository _scheduleRepository;
        private IRequestApprovalService _requestApprovalService;
        private IRequestFactory _factory;
        private IScheduleDictionarySaver _scheduleDictionarySaver;
        private IPersonRequestCheckAuthorization _authorization;
        private IScenarioProvider _scenarioProvider;
        private IOccupiedSeatCalculator _occupiedSeatCalculator;
        private IScheduleIsInvalidSpecification _scheduleIsInvalidSpecification;
        private IScheduleDictionaryModifiedCallback _scheduleDictionaryModifiedCallback;
        private INonBlendSkillCalculator _nonBlendSkillCalculator;
        private IBudgetDayRepository _budgetDayRepository;
        private IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
        private IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            
            CreateInfrastructure();
            CreateServices();
            CreateSchedulingResultStateHolder();
            CreatePersonAndRequest();
            CreateRepositories();
            
            _scenarioProvider = _mockRepository.StrictMock<IScenarioProvider>();
            _occupiedSeatCalculator = _mockRepository.StrictMock<IOccupiedSeatCalculator>();
            _scheduleIsInvalidSpecification = _mockRepository.StrictMock<IScheduleIsInvalidSpecification>();
            _scheduleDictionaryModifiedCallback = _mockRepository.DynamicMock<IScheduleDictionaryModifiedCallback>();
            _nonBlendSkillCalculator = _mockRepository.StrictMock<INonBlendSkillCalculator>();

            _budgetDayRepository = _mockRepository.StrictMock<IBudgetDayRepository>();
            _scheduleProjectionReadOnlyRepository = _mockRepository.StrictMock<IScheduleProjectionReadOnlyRepository>();
            _updateScheduleProjectionReadModel = _mockRepository.StrictMock<IUpdateScheduleProjectionReadModel>();

            Expect.Call(_absenceRequest.Person).Return(_person).Repeat.Any();
            _target = new NewAbsenceRequestConsumer(_scheduleRepository, _personAbsenceAccountProvider,
                                                    _scenarioProvider, _personRequestRepository,
                                                    _occupiedSeatCalculator, _schedulingResultStateHolder, _merger,
                                                    _factory, _scheduleDictionarySaver,
                                                    _scheduleIsInvalidSpecification, _authorization,
                                                    _scheduleDictionaryModifiedCallback, _nonBlendSkillCalculator,
                                                    _budgetDayRepository, _scheduleProjectionReadOnlyRepository,
                                                    _updateScheduleProjectionReadModel);
        }

        private void CreateServices()
        {
            _requestApprovalService = _mockRepository.StrictMock<IRequestApprovalService>();
            _merger = _mockRepository.StrictMock<IAbsenceRequestOpenPeriodMerger>();
        }

        private void CreateInfrastructure()
        {
            _unitOfWorkFactory = _mockRepository.StrictMock<IUnitOfWorkFactory>();
            _unitOfWork = _mockRepository.StrictMock<IUnitOfWork>();
            _authorization = new PersonRequestAuthorizationCheckerForTest();
            _factory = _mockRepository.StrictMock<IRequestFactory>();
        }

        private void CreatePersonAndRequest()
        {
            _workflowControlSet = _mockRepository.StrictMock<IWorkflowControlSet>();
            _person = new Person{Name = new Name("John","Doe")};
            _person.WorkflowControlSet = _workflowControlSet;
            _personRequest = _mockRepository.StrictMock<IPersonRequest>();
            _absenceRequest = _mockRepository.StrictMock<IAbsenceRequest>();
        }

        private void CreateSchedulingResultStateHolder()
        {
            _schedulingResultStateHolder = new SchedulingResultStateHolder();
            _scheduleDictionary = _mockRepository.DynamicMock<IScheduleDictionary>();
            _scheduleDictionarySaver = _mockRepository.StrictMock<IScheduleDictionarySaver>();
            _schedulingResultStateHolder.Schedules = _scheduleDictionary;
        }

        private void CreateRepositories()
        {
            _scheduleRepository = _mockRepository.StrictMock<IScheduleRepository>();
            _personRequestRepository = _mockRepository.StrictMock<IPersonRequestRepository>();
            _personAbsenceAccountProvider = _mockRepository.StrictMock<IPersonAbsenceAccountProvider>();
        }

        [Test]
        public void VerifyCanConsumeMessageAndReturnIfNotNew()
        {
            SetupAuthorizationAndDataSource();
            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(false);

                Expect.Call(_personRequestRepository.Get(_message.PersonRequestId)).Return(_personRequest);
                Expect.Call(_personRequest.Request).Return(_absenceRequest).Repeat.Any();
                Expect.Call(_personRequest.IsNew).Return(false);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [Test]
        public void VerifyCanConsumeMessageAndReturnIfNotAbsenceRequest()
        {
            SetupAuthorizationAndDataSource();
            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(false);

                IShiftTradeRequest shiftTradeRequest = _mockRepository.StrictMock<IShiftTradeRequest>();
                Expect.Call(_personRequestRepository.Get(_message.PersonRequestId)).Return(_personRequest);
                Expect.Call(_personRequest.IsNew).Return(true);
                Expect.Call(_personRequest.Request).Return(shiftTradeRequest);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [Test]
        public void VerifyCanConsumeMessageAndReturnIfRequestIsNull()
        {
            SetupAuthorizationAndDataSource();
            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(false);

                Expect.Call(_personRequestRepository.Get(_message.PersonRequestId)).Return(_personRequest);
                Expect.Call(_personRequest.IsNew).Return(true);
                Expect.Call(_personRequest.Request).Return(null);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyAbsenceRequestCanBeSetToPending()
        {
            SetupAuthorizationAndDataSource();
            
            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            ILoadSchedulingStateHolderForResourceCalculation schedulingStateHolderForResourceCalculation =
                _mockRepository.StrictMock<ILoadSchedulingStateHolderForResourceCalculation>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();
            
            schedulingStateHolderForResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson>{_person});
            var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

            var absenceRequestOpenDatePeriod = _mockRepository.StrictMock<IAbsenceRequestOpenPeriod>();
            var openAbsenceRequestPeriodExtractor = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodExtractor>();
            var openAbsenceRequestPeriodProjection = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodProjection>();
            var periodList = new List<IAbsenceRequestOpenPeriod> {absenceRequestOpenDatePeriod};
            IPersonAbsenceAccount personAbsenceAccount = new PersonAbsenceAccount(_person, _absence);
            var personAbsenceAccountList = new List<IPersonAbsenceAccount>
                                               {
                                                   personAbsenceAccount
                                               };
            
            using(_mockRepository.Record())
            {
                PrepareUnitOfWork(true);
                PrepareAbsenceRequest();
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _person)).Return(
                    new List<IBusinessRuleResponse>());
                Expect.Call(_personAbsenceAccountProvider.Find(_person)).Return(personAccountCollection);
                Expect.Call(personAccountCollection.GetEnumerator()).Return(personAbsenceAccountList.GetEnumerator()).
                    Repeat.Any();
                Expect.Call(personAccountCollection.Find(_absence)).Return(personAbsenceAccount);
                Expect.Call(_merger.Merge(periodList)).Return(absenceRequestOpenDatePeriod);
                Expect.Call(_workflowControlSet.GetExtractorForAbsence(_absence)).Return(
                    openAbsenceRequestPeriodExtractor);
                openAbsenceRequestPeriodExtractor.ViewpointDate = DateOnly.Today;
                Expect.Call(openAbsenceRequestPeriodExtractor.Projection).Return(openAbsenceRequestPeriodProjection);
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedProcess(null, null)).IgnoreArguments().Return(
                    processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList(_schedulingResultStateHolder, null,
                                                                                  null,null))
                    .IgnoreArguments().Return(validatorList);
                Expect.Call(_factory.GetSchedulingLoader(_schedulingResultStateHolder)).Return(
                    schedulingStateHolderForResourceCalculation);
                Expect.Call(_factory.GetRequestApprovalService(null, _schedulingResultStateHolder, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);
                Expect.Call(_unitOfWork.Merge(personAbsenceAccount)).Return(personAbsenceAccount);

                processAbsenceRequest.Process(null, _absenceRequest, _authorization, validatorList);
                
                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
                Expect.Call(() => _updateScheduleProjectionReadModel.Execute(_scenario, _period, _person));
                ExpectLoadOfSchedules();
                ExpectPersistOfDictionary();
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldSetRequestToPendingWhenInvalidSchedule()
        {
            SetupAuthorizationAndDataSource();

            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            ILoadSchedulingStateHolderForResourceCalculation schedulingStateHolderForResourceCalculation =
                _mockRepository.StrictMock<ILoadSchedulingStateHolderForResourceCalculation>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            schedulingStateHolderForResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
            var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

            var absenceRequestOpenDatePeriod = _mockRepository.StrictMock<IAbsenceRequestOpenPeriod>();
            var openAbsenceRequestPeriodExtractor = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodExtractor>();
            var openAbsenceRequestPeriodProjection = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodProjection>();
            var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };
            IPersonAbsenceAccount personAbsenceAccount = new PersonAbsenceAccount(_person, _absence);
            var personAbsenceAccountList = new List<IPersonAbsenceAccount>
                                               {
                                                   personAbsenceAccount
                                               };

            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(true);
                PrepareAbsenceRequest();
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _person)).Return(
                    new List<IBusinessRuleResponse>());
                Expect.Call(_personAbsenceAccountProvider.Find(_person)).Return(personAccountCollection);
                Expect.Call(personAccountCollection.GetEnumerator()).Return(personAbsenceAccountList.GetEnumerator()).
                    Repeat.Any();
                Expect.Call(personAccountCollection.Find(_absence)).Return(personAbsenceAccount);
                Expect.Call(_merger.Merge(periodList)).Return(absenceRequestOpenDatePeriod);
                Expect.Call(_workflowControlSet.GetExtractorForAbsence(_absence)).Return(
                    openAbsenceRequestPeriodExtractor);
                openAbsenceRequestPeriodExtractor.ViewpointDate = DateOnly.Today;
                Expect.Call(openAbsenceRequestPeriodExtractor.Projection).Return(openAbsenceRequestPeriodProjection);
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedProcess(null, null)).IgnoreArguments().Return(
                    processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList(_schedulingResultStateHolder, null,
                                                                                  null, null))
                    .IgnoreArguments().Return(validatorList);
                Expect.Call(_factory.GetSchedulingLoader(_schedulingResultStateHolder)).Return(
                    schedulingStateHolderForResourceCalculation);
                Expect.Call(_factory.GetRequestApprovalService(null, _schedulingResultStateHolder, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);
                Expect.Call(_unitOfWork.Merge(personAbsenceAccount)).Return(personAbsenceAccount);
                Expect.Call(absenceRequestValidator.Validate(_absenceRequest)).Return(true);
                Expect.Call(_absenceRequest.Parent).Return(_personRequest);
                Expect.Call(_personRequest.Pending);

                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(true);
                Expect.Call(() => _updateScheduleProjectionReadModel.Execute(_scenario, _period, _person));
                ExpectLoadOfSchedules();
                ExpectPersistOfDictionary();
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldLogErrorWhenInvalidScheduleInPersist()
        {
            SetupAuthorizationAndDataSource();

            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            ILoadSchedulingStateHolderForResourceCalculation schedulingStateHolderForResourceCalculation =
                _mockRepository.StrictMock<ILoadSchedulingStateHolderForResourceCalculation>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            schedulingStateHolderForResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
            var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

            var absenceRequestOpenDatePeriod = _mockRepository.StrictMock<IAbsenceRequestOpenPeriod>();
            var openAbsenceRequestPeriodExtractor = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodExtractor>();
            var openAbsenceRequestPeriodProjection = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodProjection>();
            var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };
            IPersonAbsenceAccount personAbsenceAccount = new PersonAbsenceAccount(_person, _absence);
            var personAbsenceAccountList = new List<IPersonAbsenceAccount>
                                               {
                                                   personAbsenceAccount
                                               };
        	var changes = new DifferenceCollection<IPersistableScheduleData>();

            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(false);
                PrepareAbsenceRequest();
                ExpectLoadOfSchedules();
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _person)).Return(
                    new List<IBusinessRuleResponse>());
                Expect.Call(_personAbsenceAccountProvider.Find(_person)).Return(personAccountCollection);
                Expect.Call(personAccountCollection.GetEnumerator()).Return(personAbsenceAccountList.GetEnumerator()).
                    Repeat.Any();
                Expect.Call(personAccountCollection.Find(_absence)).Return(personAbsenceAccount);
                Expect.Call(_merger.Merge(periodList)).Return(absenceRequestOpenDatePeriod);
                Expect.Call(_workflowControlSet.GetExtractorForAbsence(_absence)).Return(
                    openAbsenceRequestPeriodExtractor);
                openAbsenceRequestPeriodExtractor.ViewpointDate = DateOnly.Today;
                Expect.Call(openAbsenceRequestPeriodExtractor.Projection).Return(openAbsenceRequestPeriodProjection);
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedProcess(null, null)).IgnoreArguments().Return(
                    processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList(_schedulingResultStateHolder, null,
                                                                                  null, null))
                    .IgnoreArguments().Return(validatorList);
                Expect.Call(_factory.GetSchedulingLoader(_schedulingResultStateHolder)).Return(
                    schedulingStateHolderForResourceCalculation);
                Expect.Call(_factory.GetRequestApprovalService(null, _schedulingResultStateHolder, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);
                
                processAbsenceRequest.Process(null, _absenceRequest, _authorization, validatorList);

                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
            	Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(changes);
				Expect.Call(_scheduleDictionarySaver.MarkForPersist(_unitOfWork, _scheduleRepository, changes)).Throw(new ValidationException());
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyNoValidPersonAccountFoundContinuesRequest()
        {
            SetupAuthorizationAndDataSource();
            
            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            ILoadSchedulingStateHolderForResourceCalculation schedulingStateHolderForResourceCalculation =
                _mockRepository.StrictMock<ILoadSchedulingStateHolderForResourceCalculation>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

            var absenceRequestOpenDatePeriod = _mockRepository.StrictMock<IAbsenceRequestOpenPeriod>();
            var openAbsenceRequestPeriodExtractor = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodExtractor>();
            var openAbsenceRequestPeriodProjection = _mockRepository.StrictMock<IOpenAbsenceRequestPeriodProjection>();
            var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(true);
                PrepareAbsenceRequest();

                schedulingStateHolderForResourceCalculation.Execute(_scenario,
                                                                    _period.ChangeStartTime(TimeSpan.FromDays(-1)),
                                                                    new List<IPerson> {_person});
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _person)).Return(
                    new List<IBusinessRuleResponse>());

                Expect.Call(_personAbsenceAccountProvider.Find(_person)).Return(personAccountCollection);
                Expect.Call(personAccountCollection.GetEnumerator()).Return(
                    new List<IPersonAbsenceAccount>().GetEnumerator());
                Expect.Call(personAccountCollection.Find(_absence)).Return(null);
                Expect.Call(_merger.Merge(periodList)).Return(absenceRequestOpenDatePeriod);
                Expect.Call(_workflowControlSet.GetExtractorForAbsence(_absence)).Return(
                    openAbsenceRequestPeriodExtractor);
                openAbsenceRequestPeriodExtractor.ViewpointDate = DateOnly.Today;
                Expect.Call(openAbsenceRequestPeriodExtractor.Projection).Return(openAbsenceRequestPeriodProjection);
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedProcess(null, null)).IgnoreArguments().Return(
                    processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList(_schedulingResultStateHolder, null,
                                                                                  null, null))
                    .IgnoreArguments().Return(validatorList);
                Expect.Call(_factory.GetSchedulingLoader(_schedulingResultStateHolder)).Return(
                    schedulingStateHolderForResourceCalculation);
                Expect.Call(_factory.GetRequestApprovalService(null, _schedulingResultStateHolder, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);

                processAbsenceRequest.Process(null, _absenceRequest, _authorization, validatorList);

                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
                Expect.Call(() => _updateScheduleProjectionReadModel.Execute(_scenario, _period, _person));
                ExpectPersistOfDictionary();
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        private void ExpectLoadOfSchedules()
        {
            var scheduleRange = _mockRepository.StrictMock<IScheduleRange>();
            Expect.Call(_scheduleDictionary[_person]).Return(scheduleRange).Repeat.AtLeastOnce();
            Expect.Call(scheduleRange.ScheduledDayCollection(new DateOnlyPeriod())).IgnoreArguments().Return(
                new List<IScheduleDay>()).Repeat.Any();
            Expect.Call(scheduleRange.Period).Return(_period).Repeat.AtLeastOnce();
        }

        private void ExpectPersistOfDictionary()
        {
        	var changes = new DifferenceCollection<IPersistableScheduleData>();
        	Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(changes);
			_scheduleDictionarySaver.MarkForPersist(_unitOfWork, _scheduleRepository, changes);
            LastCall.Return(new ScheduleDictionaryPersisterResult { ModifiedEntities = new IPersistableScheduleData[] { }, AddedEntities = new IPersistableScheduleData[] { }, DeletedEntities = new IPersistableScheduleData[] { } });
        }

        [Test]
        public void VerifyAbsenceRequestGetsDeniedWhenNoWorkflowControlSet()
        {
            SetupAuthorizationAndDataSource();
            
            _person.WorkflowControlSet = null;
            ILoadSchedulingStateHolderForResourceCalculation schedulingStateHolderForResourceCalculation =
                _mockRepository.StrictMock<ILoadSchedulingStateHolderForResourceCalculation>();

            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(true);
                PrepareAbsenceRequest();

                schedulingStateHolderForResourceCalculation.Execute(_scenario,
                                                                    _period.ChangeStartTime(TimeSpan.FromDays(-1)),
                                                                    new List<IPerson> {_person});
                Expect.Call(_absenceRequest.Parent).Return(_personRequest);
                _personRequest.Deny(null, "RequestDenyReasonNoWorkflow", _authorization);

                Expect.Call(_factory.GetSchedulingLoader(_schedulingResultStateHolder)).Return(
                    schedulingStateHolderForResourceCalculation);
                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
                Expect.Call(() => _updateScheduleProjectionReadModel.Execute(_scenario, _period, _person));
                ExpectPersistOfDictionary();
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [Test]
        public void ShouldStillDenyIfScheduleIsInvalid()
        {
            SetupAuthorizationAndDataSource();

            _person.WorkflowControlSet = null;
            ILoadSchedulingStateHolderForResourceCalculation schedulingStateHolderForResourceCalculation =
                _mockRepository.StrictMock<ILoadSchedulingStateHolderForResourceCalculation>();

            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(true);
                PrepareAbsenceRequest();

                schedulingStateHolderForResourceCalculation.Execute(_scenario,
                                                                    _period.ChangeStartTime(TimeSpan.FromDays(-1)),
                                                                    new List<IPerson> { _person });
                Expect.Call(_absenceRequest.Parent).Return(_personRequest);
                _personRequest.Deny(null, "RequestDenyReasonNoWorkflow", _authorization);

                Expect.Call(_factory.GetSchedulingLoader(_schedulingResultStateHolder)).Return(
                    schedulingStateHolderForResourceCalculation);
                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(true);
                Expect.Call(() => _updateScheduleProjectionReadModel.Execute(_scenario, _period, _person));
                ExpectPersistOfDictionary();
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        private void PrepareAbsenceRequest()
        {
            Expect.Call(_scenarioProvider.DefaultScenario()).Return(_scenario).Repeat.AtLeastOnce();
            Expect.Call(_personRequestRepository.Get(_message.PersonRequestId)).Return(_personRequest).Repeat.AtLeastOnce();
            Expect.Call(_personRequest.IsNew).Return(true);
            Expect.Call(_personRequest.Request).Return(_absenceRequest).Repeat.Any();

            Expect.Call(_absenceRequest.Period).Return(_period).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Absence).Return(_absence).Repeat.Any();
        }

        private void SetupAuthorizationAndDataSource()
        {
            UnitOfWorkFactoryContainer.Current = _unitOfWorkFactory;
        }

        private void PrepareUnitOfWork(bool persistAll)
        {
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            Expect.Call(() => _unitOfWork.Dispose());
            if (persistAll)
            {
                Expect.Call(_unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
            }
        }
    }
}
