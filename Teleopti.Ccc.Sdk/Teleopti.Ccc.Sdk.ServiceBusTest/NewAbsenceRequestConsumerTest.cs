using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture]
	public class NewAbsenceRequestConsumerTest
	{
		private NewAbsenceRequestConsumer _target;
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;
		private IPersonRequestRepository _personRequestRepository;
		private IPersonRequest _personRequest;
		private IAbsenceRequest _absenceRequest;
		private IPerson _person;
		private readonly NewAbsenceRequestCreated _message = new NewAbsenceRequestCreated { PersonRequestId = Guid.NewGuid() };
		private readonly IScenario _scenario = new Scenario("Test");
		private readonly IAbsence _absence = new Absence { Description = new Description("Vacation", "VAC"), Tracker = Tracker.CreateDayTracker() };
		private readonly DateTimePeriod _period = new DateTimePeriod(2010, 3, 30, 2010, 3, 31);
		private readonly DateOnlyPeriod _dateOnlyPeriod = new DateOnlyPeriod(2010, 3, 30, 2010, 3, 30);
		private IWorkflowControlSet _workflowControlSet;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		//private IAbsenceRequestOpenPeriodMerger _merger;
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
		private IResourceCalculationPrerequisitesLoader _prereqLoader;
		private IPersonAccountCollection _personAccountCollection;
		private IPersonAccountUpdater _personAccountUpdater;

		[SetUp]
		public void Setup()
		{
			CreateInfrastructure();
			CreateServices();
			CreateSchedulingResultStateHolder();
			CreatePersonAndRequest();
			CreateRepositories();

			_scenarioRepository = MockRepository.GenerateMock<ICurrentScenario>();
			_scheduleIsInvalidSpecification = MockRepository.GenerateMock<IScheduleIsInvalidSpecification>();

			_alreadyAbsentSpecification = MockRepository.GenerateMock<IAlreadyAbsentSpecification>();
			_loader = MockRepository.GenerateMock<ILoadSchedulingStateHolderForResourceCalculation>();
			_prereqLoader = MockRepository.GenerateMock<IResourceCalculationPrerequisitesLoader>();
			_loaderWithoutResourceCalculation = MockRepository.GenerateMock<ILoadSchedulesForRequestWithoutResourceCalculation>();
			_budgetGroupAllowanceSpecification = MockRepository.GenerateMock<IBudgetGroupAllowanceSpecification>();
			_budgetGroupHeadCountSpecification = MockRepository.GenerateMock<IBudgetGroupHeadCountSpecification>();
			_resourceOptimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			_updateScheduleProjectionReadModel = MockRepository.GenerateMock<IUpdateScheduleProjectionReadModel>();

			var businessRules = MockRepository.GenerateMock<IBusinessRulesForPersonalAccountUpdate>();

			_personAccountUpdater = MockRepository.GenerateMock<IPersonAccountUpdater>();
			
			_validatedRequest = new ValidatedRequest { IsValid = true, ValidationErrors = "" };
			
			_absenceRequest.Stub(x => x.Person).Return(_person);
			var absenceRequestStatusUpdater = new AbsenceRequestUpdater (_personAbsenceAccountProvider,
				_prereqLoader, _scenarioRepository, _loader, _loaderWithoutResourceCalculation, _factory,
				_alreadyAbsentSpecification, _scheduleIsInvalidSpecification, _authorization, _budgetGroupHeadCountSpecification,
				_resourceOptimizationHelper, _budgetGroupAllowanceSpecification, _scheduleDictionarySaver, _personAccountUpdater,
				new FalseToggleManager(), businessRules);


			var absenceProcessor = new AbsenceRequestProcessor(absenceRequestStatusUpdater, _updateScheduleProjectionReadModel, _schedulingResultStateHolder);
			var absenceRequestWaitlistProcessor = new AbsenceRequestWaitlistProcessor(_personRequestRepository, absenceRequestStatusUpdater, _schedulingResultStateHolder, _updateScheduleProjectionReadModel);

			_target = new NewAbsenceRequestConsumer( 
				_unitOfWorkFactory, _scenarioRepository, _personRequestRepository, absenceRequestWaitlistProcessor, absenceProcessor);

			PrepareUnitOfWork();
		}

		private void CreateServices()
		{
			_requestApprovalService = MockRepository.GenerateMock<IRequestApprovalService>();
			//_merger = MockRepository.GenerateMock<IAbsenceRequestOpenPeriodMerger>();
		}

		private void CreateInfrastructure()
		{
			_unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_authorization = new PersonRequestAuthorizationCheckerForTest();
			_factory = MockRepository.GenerateMock<IRequestFactory>();
		}

		private void CreatePersonAndRequest()
		{
			_workflowControlSet = MockRepository.GenerateMock<IWorkflowControlSet>();
			_person = new Person { Name = new Name("John", "Doe") };
			_person.WorkflowControlSet = _workflowControlSet;
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_personAccountCollection = new PersonAccountCollection(_person);
			_personRequest = MockRepository.GenerateMock<IPersonRequest>();
			_absenceRequest = MockRepository.GenerateMock<IAbsenceRequest>();
		}

		private void CreateSchedulingResultStateHolder()
		{
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			_scheduleDictionarySaver = MockRepository.GenerateMock<IScheduleDifferenceSaver>();
			_schedulingResultStateHolder.Schedules = _scheduleDictionary;
		}

		private void CreateRepositories()
		{
			_personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			_personAbsenceAccountProvider = MockRepository.GenerateMock<IPersonAbsenceAccountProvider>();
			MockRepository.GenerateMock<IPersonAbsenceAccountRepository>();
		}

		[Test]
		public void VerifyCanConsumeMessageAndReturnIfNotNew()
		{
			_personRequestRepository.Stub(x => x.Get(_message.PersonRequestId)).Return(_personRequest);
			_personRequest.Stub(x => x.Request).Return(_absenceRequest).Repeat.Any();
			_personRequest.Stub(x => x.IsNew).Return(false);

			_target.Consume(_message);
			_unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}

		[Test]
		public void VerifyCanConsumeMessageAndReturnIfNotAbsenceRequest()
		{
			var shiftTradeRequest = MockRepository.GenerateMock<IShiftTradeRequest>();
			_personRequestRepository.Stub(x => x.Get(_message.PersonRequestId)).Return(_personRequest);
			_personRequest.Stub(x => x.IsNew).Return(true);
			_personRequest.Stub(x => x.Request).Return(shiftTradeRequest);

			_target.Consume(_message);
			_unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}

		[Test]
		public void VerifyCanConsumeMessageAndReturnIfRequestIsNull()
		{
			_personRequestRepository.Stub(x => x.Get(_message.PersonRequestId)).Return(_personRequest);
			_personRequest.Stub(x => x.IsNew).Return(true);
			_personRequest.Stub(x => x.Request).Return(null);

			_target.Consume(_message);
			_unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}

		[Test]
		public void VerifyAbsenceRequestCanBeSetToPending()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			ExpectLoadOfSchedules();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person))
								   .Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);

			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(
				x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture()))
											  .IgnoreArguments()
											  .Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);

			_scheduleIsInvalidSpecification.Stub(x => x.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll());
			_loaderWithoutResourceCalculation.AssertWasCalled(
				x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
			processAbsenceRequest.AssertWasCalled(
				x =>
				x.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(),
						  new RequiredForHandlingAbsenceRequest(), validatorList), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldLoadEverythingNeededForResourceCalculationWhenCheckingIntradayStaffing()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = new StaffingThresholdValidator();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			ExpectLoadOfSchedules();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person))
								   .Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(
				x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture()))
											  .IgnoreArguments()
											  .Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);

			_scheduleIsInvalidSpecification.Stub(x => x.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);

			_target.Consume(_message);

			_loaderWithoutResourceCalculation.AssertWasNotCalled(
				x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
			_loader.AssertWasCalled(
				x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
			_prereqLoader.AssertWasCalled(x => x.Execute());
		}

		[Test]
		public void ShouldSetRequestToPendingWhenInvalidSchedule()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			ExpectLoadOfSchedules();

			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person)).Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).IgnoreArguments().Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);
			absenceRequestValidator.Stub(x => x.Validate(_absenceRequest, new RequiredForHandlingAbsenceRequest())).IgnoreArguments().Return(_validatedRequest);

			_scheduleIsInvalidSpecification.Stub(x => x.IsSatisfiedBy(_schedulingResultStateHolder)).Return(true);

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll());
			_personRequest.AssertWasCalled(x => x.Pending());
			_loaderWithoutResourceCalculation.AssertWasCalled(x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSetRequestToPendingWhenAlreadyAbsent()
		{
			var processAbsenceRequest = new GrantAbsenceRequest();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person)).Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).IgnoreArguments().Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);
			_alreadyAbsentSpecification.Stub(x => x.IsSatisfiedBy(_absenceRequest)).Return(true);
			ExpectLoadOfSchedules();

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll());
			_loaderWithoutResourceCalculation.AssertWasCalled(x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
		}

		[Test]
		public void ShouldOnlyRecreateScheduleReadModelsOnApprove()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var personAbsenceAccount = new PersonAbsenceAccount(_person, _absence);
			_personAccountCollection.Add(personAbsenceAccount);
			_loaderWithoutResourceCalculation.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person });
			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person)).Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).IgnoreArguments().Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);
			_unitOfWork.Stub(x => x.Merge(personAbsenceAccount)).Return(personAbsenceAccount);
			_personRequest.Stub(x => x.IsApproved).Return(true);
			_alreadyAbsentSpecification.Stub(x => x.IsSatisfiedBy(_absenceRequest)).Return(false);
			ExpectLoadOfSchedules();
			ExpectPersistOfDictionary();

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll(), o => o.Repeat.Twice());
			_updateScheduleProjectionReadModel.AssertWasCalled(x => x.Execute(_scheduleRange, _dateOnlyPeriod));
			processAbsenceRequest.AssertWasCalled(x => x.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(), new RequiredForHandlingAbsenceRequest(), validatorList), o => o.IgnoreArguments());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSetRequestToDeniedWhenAlreadyAbsentAndAutoGrantEnabled()
		{
			var processAbsenceRequest = new GrantAbsenceRequest();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person)).Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).IgnoreArguments().Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);

			_alreadyAbsentSpecification.Stub(x => x.IsSatisfiedBy(_absenceRequest)).Return(true);
			ExpectLoadOfSchedules();

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll());
			_loaderWithoutResourceCalculation.AssertWasCalled(x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
		}

		[Test]
		public void ShouldLogErrorWhenInvalidScheduleInPersist()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };
			var changes = new DifferenceCollection<IPersistableScheduleData>();

			PrepareAbsenceRequest();
			ExpectLoadOfSchedules();
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(
				x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture()))
											  .IgnoreArguments()
											  .Return(periodList);
			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(false);
			
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person))
								   .Return(new List<IBusinessRuleResponse>());
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_scheduleIsInvalidSpecification.Stub(x => x.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);
			_scheduleDictionarySaver.Stub(x => x.SaveChanges(changes, null))
									.IgnoreArguments()
									.Throw(new ValidationException());
			_personRequest.Stub(x => x.IsApproved).Return(true);

			_target.Consume(_message);
			
			_unitOfWork.AssertWasNotCalled(x => x.PersistAll());
			_loaderWithoutResourceCalculation.AssertWasCalled(x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
		}

		[Test]
		public void VerifyNoValidPersonAccountFoundContinuesRequest()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			ExpectPersistOfDictionary();
			ExpectLoadOfSchedules();

			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person))
								   .Return(new List<IBusinessRuleResponse>());

			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);

			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(false);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(
				x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture()))
											  .IgnoreArguments()
											  .Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);

			_personRequest.Stub(x => x.IsApproved).Return(true);
			_scheduleIsInvalidSpecification.Stub(x => x.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll(), o => o.Repeat.Times(2));
			_loaderWithoutResourceCalculation.Stub(
				x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
			_updateScheduleProjectionReadModel.Stub(x => x.Execute(_scheduleRange, _dateOnlyPeriod));
			processAbsenceRequest.AssertWasCalled(
				x =>
				x.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(),
						  new RequiredForHandlingAbsenceRequest(), validatorList), o => o.IgnoreArguments());
		}

		private void ExpectLoadOfSchedules()
		{
			_scheduleRange = new ScheduleRange(_scheduleDictionary, new ScheduleParameters(_scenario, _person, _period));
			_scheduleDictionary.Stub(x => x[_person]).Return(_scheduleRange);
			_scheduleDictionary.Stub(x => x.Scenario).Return(_scenario);
		}

		private void ExpectPersistOfDictionary()
		{
			var changes = new DifferenceCollection<IPersistableScheduleData>();
			_scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(changes);
		}

		[Test]
		public void ShouldStillDenyIfScheduleIsInvalid()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();

			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			ExpectLoadOfSchedules();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person)).Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);


			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).IgnoreArguments().Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);
			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);
			_scheduleIsInvalidSpecification.Stub(x => x.IsSatisfiedBy(_schedulingResultStateHolder)).Return(false);

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll());
			processAbsenceRequest.AssertWasCalled(x => x.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(), new RequiredForHandlingAbsenceRequest(), validatorList), o => o.IgnoreArguments());
			_loaderWithoutResourceCalculation.AssertWasCalled(x => x.Execute(_scenario, _period.ChangeStartTime(TimeSpan.FromDays(-1)), new List<IPerson> { _person }));
		}

	/*	[Test]
		public void ShouldTrackAbsenceTwoTimes()
		{
			var processAbsenceRequest = MockRepository.GenerateMock<IProcessAbsenceRequest>();
			var absenceRequestValidator = MockRepository.GenerateMock<IAbsenceRequestValidator>();
			var tracker = MockRepository.GenerateMock<ITracker>();
			_absence.Tracker = tracker;

			var personAbsenceAccount = new PersonAbsenceAccount(_person, _absence);
			_personAccountCollection.Add(personAbsenceAccount);
			personAbsenceAccount.Add(new AccountTime(_dateOnlyPeriod.StartDate));
			
			var validatorList = new List<IAbsenceRequestValidator> { absenceRequestValidator };

			var absenceRequestOpenDatePeriod = MockRepository.GenerateMock<IAbsenceRequestOpenPeriod>();
			var openAbsenceRequestPeriodExtractor = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodExtractor>();
			var openAbsenceRequestPeriodProjection = MockRepository.GenerateMock<IOpenAbsenceRequestPeriodProjection>();
			var periodList = new List<IAbsenceRequestOpenPeriod> { absenceRequestOpenDatePeriod };

			PrepareAbsenceRequest();
			_requestApprovalService.Stub(x => x.ApproveAbsence(_absence, _period, _person)).Return(new List<IBusinessRuleResponse>());
			_personAbsenceAccountProvider.Stub(x => x.Find(_person)).Return(_personAccountCollection);
			_personAccountUpdater.Stub(x => x.UpdateForAbsence(_person, _absence, new DateOnly(_period.StartDateTime)))
				.Return(true);

			_workflowControlSet.Stub(x => x.GetMergedAbsenceRequestOpenPeriod(_absenceRequest)).Return(absenceRequestOpenDatePeriod);
			_workflowControlSet.Stub(x => x.GetExtractorForAbsence(_absence)).Return(openAbsenceRequestPeriodExtractor);
			openAbsenceRequestPeriodExtractor.Stub(x => x.Projection).Return(openAbsenceRequestPeriodProjection);
			openAbsenceRequestPeriodProjection.Stub(x => x.GetProjectedPeriods(new DateOnlyPeriod(), _person.PermissionInformation.Culture())).IgnoreArguments().Return(periodList);
			absenceRequestOpenDatePeriod.Stub(x => x.AbsenceRequestProcess).Return(processAbsenceRequest);
			absenceRequestOpenDatePeriod.Stub(x => x.GetSelectedValidatorList()).Return(validatorList);

			_factory.Stub(x => x.GetRequestApprovalService(null, _scenario)).IgnoreArguments().Return(_requestApprovalService);
			_unitOfWork.Stub(x => x.Merge(personAbsenceAccount)).Return(null);
			_personRequest.Stub(x => x.IsApproved).Return(true).Repeat.Twice();
			_alreadyAbsentSpecification.Stub(x => x.IsSatisfiedBy(_absenceRequest)).Return(false);
			ExpectLoadOfSchedules();
			ExpectPersistOfDictionary();

			_target.Consume(_message);
			_updateScheduleProjectionReadModel.AssertWasCalled(x => x.Execute(_scheduleRange, _dateOnlyPeriod));
			_unitOfWork.AssertWasCalled(x => x.PersistAll(), o => o.Repeat.Twice());
			tracker.AssertWasCalled(x => x.Track(personAbsenceAccount.AccountCollection().First(), _absence, new List<IScheduleDay>()), o => o.Repeat.Twice().IgnoreArguments());
		}*/

		[Test]
		public void VerifyAbsenceRequestGetsDeniedWhenNoWorkflowControlSet()
		{
			_person.WorkflowControlSet = null;

			PrepareAbsenceRequest();
			ExpectLoadOfSchedules();

			_target.Consume(_message);
			_unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

		private void PrepareAbsenceRequest()
		{
			_scenarioRepository.Stub(x => x.Current()).Return(_scenario);
			_personRequestRepository.Stub(x => x.Get(_message.PersonRequestId)).Return(_personRequest);
			_personRequest.Stub(x => x.IsNew).Return(true);
			_personRequest.Stub(x => x.Request).Return(_absenceRequest);

			_absenceRequest.Stub(x => x.Parent).Return(_personRequest);

			_absenceRequest.Stub(x => x.Period).Return(_period);
			_absenceRequest.Stub(x => x.Absence).Return(_absence);
		}

		private void PrepareUnitOfWork()
		{
			var currentUowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			currentUowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_unitOfWorkFactory.Stub(x => x.Current()).Return(currentUowFactory);
		}
	}
}
