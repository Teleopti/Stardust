using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class NewAbsenceRequestConsumer : ConsumerOf<NewAbsenceRequestCreated>
    {
        private readonly static ILog Logger = LogManager.GetLogger(typeof(NewAbsenceRequestConsumer));

	    private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonAbsenceAccountProvider _personAbsenceAccountProvider;
        private readonly ICurrentScenario _scenarioRepository;
        private readonly IPersonRequestRepository _personRequestRepository;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IAbsenceRequestOpenPeriodMerger _absenceRequestOpenPeriodMerger;
        private readonly IRequestFactory _factory;
				private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
        private readonly IScheduleIsInvalidSpecification _scheduleIsInvalidSpecification;

        private IPersonRequest _personRequest;
        private IAbsenceRequest _absenceRequest;
        private IPersonAbsenceAccount personAccount;
        
        private static readonly IsNullOrNotNewSpecification PersonRequestSpecification = new IsNullOrNotNewSpecification();
        private static readonly IsNullSpecification AbsenceRequestSpecification = new IsNullSpecification();
        private readonly DenyAbsenceRequest _denyAbsenceRequest = new DenyAbsenceRequest();
        private readonly PendingAbsenceRequest _pendingAbsenceRequest = new PendingAbsenceRequest();

        private readonly IList<LoadDataAction> _loadDataActions;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private readonly IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
    	private readonly IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
    	private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
        private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
        private readonly IAlreadyAbsentSpecification _alreadyAbsentSpecification;
        private readonly IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
	    private readonly IResourceCalculationPrerequisitesLoader _prereqLoader;
	    private IProcessAbsenceRequest _process;
    	private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public NewAbsenceRequestConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonAbsenceAccountProvider personAbsenceAccountProvider, ICurrentScenario scenarioRepository, IPersonRequestRepository personRequestRepository, ISchedulingResultStateHolder schedulingResultStateHolder, 
                                         IAbsenceRequestOpenPeriodMerger absenceRequestOpenPeriodMerger, IRequestFactory factory,
																				 IScheduleDifferenceSaver scheduleDictionarySaver, IScheduleIsInvalidSpecification scheduleIsInvalidSpecification, IPersonRequestCheckAuthorization authorization, 
                                         IResourceOptimizationHelper resourceOptimizationHelper, IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel, IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification, 
                                         ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation, ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation, IAlreadyAbsentSpecification alreadyAbsentSpecification, IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification, IResourceCalculationPrerequisitesLoader prereqLoader)
        {
	        _unitOfWorkFactory = unitOfWorkFactory;
            _personAbsenceAccountProvider = personAbsenceAccountProvider;
            _scenarioRepository = scenarioRepository;
            _personRequestRepository = personRequestRepository;
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _absenceRequestOpenPeriodMerger = absenceRequestOpenPeriodMerger;
            _factory = factory;
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _scheduleIsInvalidSpecification = scheduleIsInvalidSpecification;
            _authorization = authorization;
    		_resourceOptimizationHelper = resourceOptimizationHelper;
            _updateScheduleProjectionReadModel = updateScheduleProjectionReadModel;
    		_budgetGroupAllowanceSpecification = budgetGroupAllowanceSpecification;
    		_loadSchedulingStateHolderForResourceCalculation = loadSchedulingStateHolderForResourceCalculation;
    	    _loadSchedulesForRequestWithoutResourceCalculation = loadSchedulesForRequestWithoutResourceCalculation;
    	    _alreadyAbsentSpecification = alreadyAbsentSpecification;
    	    _budgetGroupHeadCountSpecification = budgetGroupHeadCountSpecification;
			_prereqLoader = prereqLoader;

			_loadDataActions = new List<LoadDataAction>
                                   {
                                       LoadAndCheckPersonRequest,
                                       GetAndCheckAbsenceRequest,
                                       LoadDefaultScenario
                                   };
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("New instance of consumer was created");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void Consume(NewAbsenceRequestCreated message)
        {
            if(Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Consuming message for person request with Id = {0}. (Message timestamp = {1})",
                                   message.PersonRequestId, message.Timestamp);
            }

            using (IUnitOfWork unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
	            foreach (var action in _loadDataActions)
	            {
		            if (!action.Invoke(message))
		            {
			            ClearStateHolder();
			            return;
		            }
	            }

	            var agentTimeZone = _absenceRequest.Person.PermissionInformation.DefaultTimeZone();
	            var dateOnlyPeriod = _absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

	            IEnumerable<IAbsenceRequestValidator> validatorList = null;
	            IPersonAccountBalanceCalculator personAccountBalanceCalculator = null;
	            IRequestApprovalService requestApprovalServiceScheduler = null;
	            var undoRedoContainer = new UndoRedoContainer(400);

	            if (!HasWorkflowControlSet())
	            {
		            denyAbsenceRequest(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoWorkflow",
			            _absenceRequest.Person.PermissionInformation.Culture()));

		            if (Logger.IsDebugEnabled)
		            {
			            Logger.DebugFormat(CultureInfo.CurrentCulture,
				            "No workflow control set defined for {0}, {1} (PersonId = {2}). The request with Id = {3} will be denied.",
				            _absenceRequest.Person.EmploymentNumber, _absenceRequest.Person.Name,
				            _absenceRequest.Person.Id, message.PersonRequestId);
		            }
	            }

	            if (HasWorkflowControlSet())
	            {
		            IOpenAbsenceRequestPeriodExtractor extractor =
			            _absenceRequest.Person.WorkflowControlSet.GetExtractorForAbsence(_absenceRequest.Absence);
		            extractor.ViewpointDate = DateOnly.Today;

		            var openPeriods = extractor.Projection.GetProjectedPeriods(dateOnlyPeriod,
			            _absenceRequest.Person.PermissionInformation.Culture());

		            var mergedPeriod = _absenceRequestOpenPeriodMerger.Merge(openPeriods);

		            validatorList = mergedPeriod.GetSelectedValidatorList();

		            _process = mergedPeriod.AbsenceRequestProcess;

		            LoadDataForResourceCalculation(validatorList);

		            _schedulingResultStateHolder.Schedules.TakeSnapshot();
		            _schedulingResultStateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);


		            IPersonAccountCollection allAccounts = _personAbsenceAccountProvider.Find(_absenceRequest.Person);
		            foreach (IPersonAbsenceAccount personAbsenceAccount in allAccounts)
		            {
			            undoRedoContainer.SaveState(personAbsenceAccount);
		            }

		            personAccount = allAccounts.Find(_absenceRequest.Absence); //Find for all later periods
		            if (personAccount == null)
		            {
			            if (_absenceRequest.Absence.Tracker != null)
			            {
				            if (Logger.IsDebugEnabled)
				            {
					            Logger.DebugFormat(CultureInfo.CurrentCulture,
						            "No person account defined for {0}, {1} (PersonId = {2}) with absence {4} for {5}. (Request Id = {3})",
						            _absenceRequest.Person.EmploymentNumber, _absenceRequest.Person.Name,
						            _absenceRequest.Person.Id, message.PersonRequestId,
						            _absenceRequest.Absence.Description,
						            dateOnlyPeriod.StartDate);
				            }
			            }
			            personAccountBalanceCalculator = new EmptyPersonAccountBalanceCalculator(_absenceRequest.Absence);
		            }
		            else
		            {
			            TrackAccounts(personAccount, dateOnlyPeriod);
			            var affectedAccounts =
				            personAccount.Find(new DateOnlyPeriod(dateOnlyPeriod.StartDate, DateOnly.MaxValue));
			            //We must have the current and all after...
			            personAccountBalanceCalculator = new PersonAccountBalanceCalculator(affectedAccounts);
		            }

		            var alreadyAbsent = personAlreadyAbsentDuringRequestPeriod();
		            var allNewRules = NewBusinessRuleCollection.Minimum();
		            requestApprovalServiceScheduler = _factory.GetRequestApprovalService(allNewRules,
			            _scenarioRepository.
				            Current());
		            var brokenBusinessRules = requestApprovalServiceScheduler.ApproveAbsence(_absenceRequest.Absence,
			            _absenceRequest.Period,
			            _absenceRequest.Person);
		            if (Logger.IsDebugEnabled)
		            {
			            foreach (var brokenBusinessRule in brokenBusinessRules)
			            {
				            Logger.DebugFormat("A rule was broken: {0}", brokenBusinessRule.Message);
			            }

			            Logger.Debug("Simulated approving absence successfully");
		            }

		            if (_process.GetType() == typeof (GrantAbsenceRequest) && alreadyAbsent)
		            {
			            denyAbsenceRequest(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonAlreadyAbsent",
				            _absenceRequest.Person.PermissionInformation.Culture()));

			            if (Logger.IsDebugEnabled)
			            {
				            Logger.DebugFormat(CultureInfo.CurrentCulture,
					            "The person is already absent during the absence request period. {0}, {1} (PersonId = {2}). The request with Id = {3} will be denied.",
					            _absenceRequest.Person.EmploymentNumber, _absenceRequest.Person.Name,
					            _absenceRequest.Person.Id,
					            message.PersonRequestId);
			            }
		            }

		            HandleInvalidSchedule();
	            }

	            //Will issue a rollback for simulated schedule data
	            if (Logger.IsInfoEnabled)
	            {
		            Logger.InfoFormat("The following process will be used for request {0}: {1}", message.PersonRequestId,
			            _process.GetType());
	            }

	            _process.Process(null, _absenceRequest,
		            new RequiredForProcessingAbsenceRequest(undoRedoContainer,
			            requestApprovalServiceScheduler, _authorization, ()
				            =>
			            {
				            if (personAccount != null)
					            TrackAccounts(personAccount, dateOnlyPeriod);
			            }),
		            new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,
			            personAccountBalanceCalculator,
			            _resourceOptimizationHelper,
			            _budgetGroupAllowanceSpecification,
			            _budgetGroupHeadCountSpecification),
		            validatorList);

	            if (_personRequest.IsApproved)
	            {
		            try
		            {
			            PersistScheduleChanges(_absenceRequest.Person);
		            }
		            catch (ValidationException validationException)
		            {
			            Logger.Error("A validation error occurred. Review the error log. Processing cannot continue.",
				            validationException);
			            ClearStateHolder();
			            return;
		            }

		            //Ugly fix to get the number updated for person account. Don't try this at home!
		            if (personAccount != null)
		            {
			            TrackAccounts(personAccount, dateOnlyPeriod);
			            unitOfWork.Merge(personAccount);
		            }
	            }
	            unitOfWork.PersistAll();

	            updateScheduleReadModelsIfRequestWasApproved(unitOfWork, dateOnlyPeriod);
            }
	        ClearStateHolder();
        }

    	private void updateScheduleReadModelsIfRequestWasApproved(IUnitOfWork unitOfWork, DateOnlyPeriod dateOnlyPeriod)
    	{
    		if (_personRequest.IsApproved)
    		{
    			_updateScheduleProjectionReadModel.Execute(_schedulingResultStateHolder.Schedules[_absenceRequest.Person], dateOnlyPeriod);

    			unitOfWork.PersistAll();
    		}
    	}

    	private bool personAlreadyAbsentDuringRequestPeriod()
    	{
    		return _alreadyAbsentSpecification.IsSatisfiedBy(_absenceRequest);
    	}

    	private void denyAbsenceRequest(string reasonResourceKey)
    	{
			_denyAbsenceRequest.DenyReason = reasonResourceKey;
			_process = _denyAbsenceRequest;
    	}

    	private void PersistScheduleChanges(IPerson person)
        {
					_scheduleDictionarySaver.SaveChanges(_schedulingResultStateHolder.Schedules.DifferenceSinceSnapshot(), (IUnvalidatedScheduleRangeUpdate) _schedulingResultStateHolder.Schedules[person]);
        }

        private void HandleInvalidSchedule()
        {
            if (_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder))
            {
                if (_process.GetType() != typeof (DenyAbsenceRequest))
                {
                    _process = _pendingAbsenceRequest;
                }
            }
        }

        private bool HasWorkflowControlSet()
        {
            return _absenceRequest.Person.WorkflowControlSet != null;
        }

        private void ClearStateHolder()
        {
            _schedulingResultStateHolder.Dispose();
            _schedulingResultStateHolder = null;
        }

        private void TrackAccounts(IPersonAbsenceAccount personAbsenceAccount, DateOnlyPeriod period)
        {
            var scheduleRange = _schedulingResultStateHolder.Schedules[_absenceRequest.Person];
            var rangePeriod = scheduleRange.Period.ToDateOnlyPeriod(_absenceRequest.Person.PermissionInformation.DefaultTimeZone());
            
            foreach (IAccount account in personAbsenceAccount.Find(period))
            {
                var intersectingPeriod = account.Period().Intersection(rangePeriod);
                if (intersectingPeriod.HasValue)
                {
                    IList<IScheduleDay> scheduleDays =
                        new List<IScheduleDay>(scheduleRange.ScheduledDayCollection(intersectingPeriod.Value));

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.InfoFormat("Remaining before tracking: {0}", account.Remaining);
                    }
                    _absenceRequest.Absence.Tracker.Track(account, _absenceRequest.Absence, scheduleDays);
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.InfoFormat("Remaining after tracking: {0}", account.Remaining);
                    }
                }
            }
        }

        private void LoadDataForResourceCalculation(IEnumerable<IAbsenceRequestValidator> validatorList)
        {
            var shouldLoadDataForResourceCalculation = validatorList != null && validatorList.Any(v => typeof(StaffingThresholdValidator) == v.GetType());
            if (shouldLoadDataForResourceCalculation)
            {
                DateTimePeriod periodForResourceCalc = _absenceRequest.Period.ChangeStartTime(TimeSpan.FromDays(-1));
				_prereqLoader.Execute();
                _loadSchedulingStateHolderForResourceCalculation.Execute(_scenarioRepository.Current(),
                                                                         periodForResourceCalc,
                                                                         new List<IPerson> {_absenceRequest.Person});
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Loaded schedules and data needed for resource calculation. (Period = {0})",
                                       periodForResourceCalc);
                }
            }
            else
            {
                DateTimePeriod periodForResourceCalc = _absenceRequest.Period.ChangeStartTime(TimeSpan.FromDays(-1));
                _loadSchedulesForRequestWithoutResourceCalculation.Execute(_scenarioRepository.Current(),
                                                                         periodForResourceCalc,
                                                                         new List<IPerson> { _absenceRequest.Person });
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Loaded schedules and data needed for absence request handling. (Period = {0})",
                                       periodForResourceCalc);
                }
            }
        }

        private bool LoadDefaultScenario(NewAbsenceRequestCreated message)
        {
            var defaultScenario = _scenarioRepository.Current();
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", defaultScenario.Description,
                                   defaultScenario.Id);
            }
            return true;
        }

        private bool GetAndCheckAbsenceRequest(NewAbsenceRequestCreated message)
        {
            _absenceRequest = _personRequest.Request as IAbsenceRequest;
            if (AbsenceRequestSpecification.IsSatisfiedBy(_absenceRequest))
            {
                if (Logger.IsWarnEnabled)
                {
                    Logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
                                      message.PersonRequestId);
                }
                return false;
            }
            return true;
        }

        private bool LoadAndCheckPersonRequest(NewAbsenceRequestCreated message)
        {
            _personRequest = _personRequestRepository.Get(message.PersonRequestId);
            if (PersonRequestSpecification.IsSatisfiedBy(_personRequest))
            {
                if (Logger.IsWarnEnabled)
                {
                    Logger.WarnFormat(
                        "No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
                        message.PersonRequestId);
                }
                return false;
            }
            return true;
        }

        private class IsNullOrNotNewSpecification : Specification<IPersonRequest>
        {
            public override bool IsSatisfiedBy(IPersonRequest obj)
            {
                return (obj == null || !obj.IsNew);
            }
        }

        private class IsNullSpecification : Specification<IAbsenceRequest>
        {
            public override bool IsSatisfiedBy(IAbsenceRequest obj)
            {
                return (obj == null);
            }
        }

        private delegate bool LoadDataAction(NewAbsenceRequestCreated message);
    }
}
