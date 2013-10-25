using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Sdk.ServiceBus;
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
				private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private IUnitOfWork _unitOfWork;
        private IPersonRequestRepository _personRequestRepository;
        private IPersonRequest _personRequest;
        private IAbsenceRequest _absenceRequest;
        private IPerson _person;
        private readonly NewAbsenceRequestCreated _message = new NewAbsenceRequestCreated { PersonRequestId = Guid.NewGuid() };
        private readonly IScenario _scenario = new Scenario("Test");
        private readonly IAbsence _absence = new Absence {Description = new Description("Vacation", "VAC"), Tracker = Tracker.CreateDayTracker()};
        private readonly DateTimePeriod _period = new DateTimePeriod(2010,3,30,2010,3,31);
		private readonly DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(2010,3,30,2010,3,30);
        private IWorkflowControlSet _workflowControlSet;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IAbsenceRequestOpenPeriodMerger _merger;
        private IScheduleDictionary _scheduleDictionary;
        private IPersonAbsenceAccountProvider _personAbsenceAccountProvider;
        private IRequestApprovalService _requestApprovalService;
        private IRequestFactory _factory;
				private IScheduleDifferenceSaver _scheduleDictionarySaver;
        private IPersonRequestCheckAuthorization _authorization;
        private ICurrentScenario _scenarioRepository;
        private IScheduleIsInvalidSpecification _scheduleIsInvalidSpecification;
        private IAlreadyAbsentSpecification _alreadyAbsentSpecification;
        private IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
        private IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
        private IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
    	private ILoadSchedulingStateHolderForResourceCalculation _loader;
    	private IResourceOptimizationHelper _resourceOptimizationHelper;
    	private IScheduleRange _scheduleRange;
        private IValidatedRequest _validatedRequest;
        private ILoadSchedulesForRequestWithoutResourceCalculation _loaderWithoutResourceCalculation;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            
            CreateInfrastructure();
            CreateServices();
            CreateSchedulingResultStateHolder();
            CreatePersonAndRequest();
            CreateRepositories();
            
            _scenarioRepository = _mockRepository.StrictMock<ICurrentScenario>();
            _scheduleIsInvalidSpecification = _mockRepository.DynamicMock<IScheduleIsInvalidSpecification>();
            
            _alreadyAbsentSpecification = _mockRepository.DynamicMock<IAlreadyAbsentSpecification>();
            _loader = _mockRepository.DynamicMock<ILoadSchedulingStateHolderForResourceCalculation>();
            _loaderWithoutResourceCalculation = _mockRepository.DynamicMock<ILoadSchedulesForRequestWithoutResourceCalculation>();
            _budgetGroupAllowanceSpecification = _mockRepository.StrictMock<IBudgetGroupAllowanceSpecification>();
    	    _budgetGroupHeadCountSpecification = _mockRepository.DynamicMock<IBudgetGroupHeadCountSpecification>();
            _resourceOptimizationHelper = _mockRepository.StrictMock<IResourceOptimizationHelper>();
            _updateScheduleProjectionReadModel = _mockRepository.StrictMock<IUpdateScheduleProjectionReadModel>();

            _validatedRequest = new ValidatedRequest();
    	    _validatedRequest.IsValid = true;
    	    _validatedRequest.ValidationErrors = "";

            Expect.Call(_absenceRequest.Person).Return(_person).Repeat.Any();
            _target = new NewAbsenceRequestConsumer(_unitOfWorkFactory, _personAbsenceAccountProvider,
                                                    _scenarioRepository, _personRequestRepository,
                                                    _schedulingResultStateHolder, _merger, _factory,
                                                    _scheduleDictionarySaver, _scheduleIsInvalidSpecification,
                                                    _authorization, _resourceOptimizationHelper, 
                                                    _updateScheduleProjectionReadModel, _budgetGroupAllowanceSpecification, _loader, _loaderWithoutResourceCalculation, _alreadyAbsentSpecification, 
                                                    _budgetGroupHeadCountSpecification);
        }

        private void CreateServices()
        {
            _requestApprovalService = _mockRepository.StrictMock<IRequestApprovalService>();
            _merger = _mockRepository.StrictMock<IAbsenceRequestOpenPeriodMerger>();
        }

        private void CreateInfrastructure()
        {
            _unitOfWorkFactory = _mockRepository.StrictMock<ICurrentUnitOfWorkFactory>();
            _unitOfWork = _mockRepository.StrictMock<IUnitOfWork>();
            _authorization = new PersonRequestAuthorizationCheckerForTest();
            _factory = _mockRepository.StrictMock<IRequestFactory>();
        }

        private void CreatePersonAndRequest()
        {
            _workflowControlSet = _mockRepository.DynamicMock<IWorkflowControlSet>();
            _person = new Person{Name = new Name("John","Doe")};
            _person.WorkflowControlSet = _workflowControlSet;
            _person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_personRequest = _mockRepository.DynamicMock<IPersonRequest>();
            _absenceRequest = _mockRepository.StrictMock<IAbsenceRequest>();
        }

        private void CreateSchedulingResultStateHolder()
        {
            _schedulingResultStateHolder = new SchedulingResultStateHolder();
            _scheduleDictionary = _mockRepository.DynamicMock<IScheduleDictionary>();
						_scheduleDictionarySaver = _mockRepository.StrictMock<IScheduleDifferenceSaver>();
            _schedulingResultStateHolder.Schedules = _scheduleDictionary;
        }

        private void CreateRepositories()
        {
            _personRequestRepository = _mockRepository.StrictMock<IPersonRequestRepository>();
            _personAbsenceAccountProvider = _mockRepository.StrictMock<IPersonAbsenceAccountProvider>();
        }

        [Test]
        public void VerifyCanConsumeMessageAndReturnIfNotNew()
        {
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
            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(
                    processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);
                Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);

                Expect.Call(()=>processAbsenceRequest.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(), new RequiredForHandlingAbsenceRequest(), validatorList)).IgnoreArguments();
                
                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldSetRequestToPendingWhenInvalidSchedule()
        {
            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);
                Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);
                Expect.Call(absenceRequestValidator.Validate(_absenceRequest, new RequiredForHandlingAbsenceRequest())).IgnoreArguments().Return(_validatedRequest);
                Expect.Call(_absenceRequest.Parent).Return(_personRequest);
                Expect.Call(_personRequest.Pending);

                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(true);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSetRequestToPendingWhenAlreadyAbsent()
		{
			IProcessAbsenceRequest processAbsenceRequest = new GrantAbsenceRequest();
			IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
			IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
				Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
					IgnoreArguments().
					Return(periodList);
				Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(processAbsenceRequest);
				Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);
				Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
					_requestApprovalService);
				Expect.Call(_absenceRequest.Parent).Return(_personRequest);
				Expect.Call(_alreadyAbsentSpecification.IsSatisfiedBy(_absenceRequest)).Return(true);
				ExpectLoadOfSchedules();
			}
			using (_mockRepository.Playback())
			{
				_target.Consume(_message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldOnlyRecreateScheduleReadModelsOnApprove()
		{
			IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
			IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
			IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
				Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
					IgnoreArguments().
					Return(periodList);
				Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(processAbsenceRequest);
				Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);

			    Expect.Call(
			        () =>
			        processAbsenceRequest.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(),
			                                      new RequiredForHandlingAbsenceRequest(), validatorList)).IgnoreArguments();

				Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
					_requestApprovalService);
				Expect.Call(_unitOfWork.Merge(personAbsenceAccount)).Return(personAbsenceAccount);
			    Expect.Call(_personRequest.IsApproved).Return(true).Repeat.Twice();
				Expect.Call(_alreadyAbsentSpecification.IsSatisfiedBy(_absenceRequest)).Return(false);
				ExpectLoadOfSchedules();
				ExpectPersistOfDictionary();
				Expect.Call(() => _updateScheduleProjectionReadModel.Execute(_scheduleRange, _dateOnlyPeriod));
				Expect.Call(_unitOfWork.PersistAll()).Return(null);
			}
			using (_mockRepository.Playback())
			{
				_target.Consume(_message);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSetRequestToDeniedWhenAlreadyAbsentAndAutoGrantEnabled()
		{
			IProcessAbsenceRequest processAbsenceRequest = new GrantAbsenceRequest();
			IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
			IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
				Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
					IgnoreArguments().
					Return(periodList);
				Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(processAbsenceRequest);
				Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);
				Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
					_requestApprovalService);
				Expect.Call(_absenceRequest.Parent).Return(_personRequest);
				Expect.Call(_alreadyAbsentSpecification.IsSatisfiedBy(_absenceRequest)).Return(true);
				ExpectLoadOfSchedules();
			}
			using (_mockRepository.Playback())
			{
				_target.Consume(_message);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldLogErrorWhenInvalidScheduleInPersist()
        {
            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);
                Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);
                
                Expect.Call(()=>processAbsenceRequest.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(), new RequiredForHandlingAbsenceRequest(), validatorList)).IgnoreArguments();

                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
            	Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(changes);
				Expect.Call(() => _scheduleDictionarySaver.SaveChanges(changes, null)).IgnoreArguments().Throw(new ValidationException());
                Expect.Call(_personRequest.IsApproved).Return(true);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyNoValidPersonAccountFoundContinuesRequest()
        {
            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
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
				ExpectPersistOfDictionary();
				ExpectLoadOfSchedules();

                _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);
                Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);

                Expect.Call(()=>processAbsenceRequest.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(), new RequiredForHandlingAbsenceRequest(), validatorList)).IgnoreArguments();
                Expect.Call(_personRequest.IsApproved).Return(true).Repeat.Twice();
                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
                Expect.Call(_unitOfWork.PersistAll()).Return(null);
                Expect.Call(() => _updateScheduleProjectionReadModel.Execute(_scheduleRange, _dateOnlyPeriod));
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        private void ExpectLoadOfSchedules()
        {
            _scheduleRange = _mockRepository.StrictMultiMock<IScheduleRange>(typeof(IUnvalidatedScheduleRangeUpdate));
            Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange).Repeat.Any();
            Expect.Call(_scheduleRange.ScheduledDayCollection(new DateOnlyPeriod())).IgnoreArguments().Return(
                new List<IScheduleDay>()).Repeat.Any();
            Expect.Call(_scheduleRange.Period).Return(_period).Repeat.Any();
        }

        private void ExpectPersistOfDictionary()
        {
        	var changes = new DifferenceCollection<IPersistableScheduleData>();
        	Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(changes);
			Expect.Call(() => _scheduleDictionarySaver.SaveChanges(changes,null)).IgnoreArguments();
        }

        [Test]
        public void ShouldStillDenyIfScheduleIsInvalid()
        {
            IProcessAbsenceRequest processAbsenceRequest = _mockRepository.StrictMock<IProcessAbsenceRequest>();
            IAbsenceRequestValidator absenceRequestValidator = _mockRepository.StrictMock<IAbsenceRequestValidator>();
            IPersonAccountCollection personAccountCollection = _mockRepository.StrictMock<IPersonAccountCollection>();

            _loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
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
                Expect.Call(openAbsenceRequestPeriodProjection.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).
                    IgnoreArguments().
                    Return(periodList);
                Expect.Call(absenceRequestOpenDatePeriod.AbsenceRequestProcess).Return(
                    processAbsenceRequest);
                Expect.Call(absenceRequestOpenDatePeriod.GetSelectedValidatorList()).Return(validatorList);
                Expect.Call(_factory.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(
                    _requestApprovalService);

                Expect.Call(() => processAbsenceRequest.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(), new RequiredForHandlingAbsenceRequest(), validatorList)).IgnoreArguments();

                Expect.Call(_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        [Test]
        public void VerifyAbsenceRequestGetsDeniedWhenNoWorkflowControlSet()
        {
            _person.WorkflowControlSet = null;
            
            using (_mockRepository.Record())
            {
                PrepareUnitOfWork(true);
                PrepareAbsenceRequest();
				ExpectLoadOfSchedules();

                Expect.Call(_absenceRequest.Parent).Return(_personRequest);
            }
            using (_mockRepository.Playback())
            {
                _target.Consume(_message);
            }
        }

        private void PrepareAbsenceRequest()
        {
            Expect.Call(_scenarioRepository.Current()).Return(_scenario).Repeat.AtLeastOnce();
            Expect.Call(_personRequestRepository.Get(_message.PersonRequestId)).Return(_personRequest).Repeat.AtLeastOnce();
            Expect.Call(_personRequest.IsNew).Return(true);
            Expect.Call(_personRequest.Request).Return(_absenceRequest).Repeat.Any();

            Expect.Call(_absenceRequest.Period).Return(_period).Repeat.AtLeastOnce();
            Expect.Call(_absenceRequest.Absence).Return(_absence).Repeat.Any();
        }

        private void PrepareUnitOfWork(bool persistAll)
        {
	        var currentUowFactory = _mockRepository.DynamicMock<IUnitOfWorkFactory>();
					Expect.Call(currentUowFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
					Expect.Call(_unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(currentUowFactory);
            Expect.Call(() => _unitOfWork.Dispose());
            if (persistAll)
            {
                Expect.Call(_unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
            }
        }
    }
}
