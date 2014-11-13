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
    public class NewAbsenceReportConsumer : ConsumerOf<NewAbsenceReportCreated>
    {
		private readonly static ILog Logger = LogManager.GetLogger(typeof(NewAbsenceReportConsumer));

	    private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
	    private readonly ICurrentScenario _scenarioRepository;
	    private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly IAbsenceRequestOpenPeriodMerger _absenceRequestOpenPeriodMerger;
        private readonly IRequestFactory _factory;
				private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
        private readonly IScheduleIsInvalidSpecification _scheduleIsInvalidSpecification;

	    private readonly PendingAbsenceRequest _pendingAbsenceRequest = new PendingAbsenceRequest();

        private readonly IList<LoadDataAction> _loadDataActions;
	    private readonly IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
	    private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
        private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
	    private readonly IResourceCalculationPrerequisitesLoader _prereqLoader;
	    private IProcessAbsenceRequest _process;
	    private readonly IPersonRepository _personRepository;

		public NewAbsenceReportConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory,ICurrentScenario scenarioRepository,ISchedulingResultStateHolder schedulingResultStateHolder, 
                                         IAbsenceRequestOpenPeriodMerger absenceRequestOpenPeriodMerger, IRequestFactory factory,
																				 IScheduleDifferenceSaver scheduleDictionarySaver, IScheduleIsInvalidSpecification scheduleIsInvalidSpecification,
                                         IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel,
                                         ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation, ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation,IResourceCalculationPrerequisitesLoader prereqLoader, IPersonRepository personRepository)
        {
	        _unitOfWorkFactory = unitOfWorkFactory;
			_scenarioRepository = scenarioRepository;
			_schedulingResultStateHolder = schedulingResultStateHolder;
            _absenceRequestOpenPeriodMerger = absenceRequestOpenPeriodMerger;
            _factory = factory;
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _scheduleIsInvalidSpecification = scheduleIsInvalidSpecification;
			_updateScheduleProjectionReadModel = updateScheduleProjectionReadModel;
			_loadSchedulingStateHolderForResourceCalculation = loadSchedulingStateHolderForResourceCalculation;
    	    _loadSchedulesForRequestWithoutResourceCalculation = loadSchedulesForRequestWithoutResourceCalculation;
			_prereqLoader = prereqLoader;
			_personRepository = personRepository;

			_loadDataActions = new List<LoadDataAction>
                                   {
                                       loadDefaultScenario
                                   };
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("New instance of consumer was created");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void Consume(NewAbsenceReportCreated message)
        {
            if(Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Consuming message for person absence report with Id = {0}. (Message timestamp = {1})",
                                   message.AbsenceId, message.Timestamp);
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
	            var person = _personRepository.FindPeople(new List<Guid> {message.PersonId}).Single();
	            var agentTimeZone = person.PermissionInformation.DefaultTimeZone();

				//create one full day period
				var fullDayTimeSpanStart = new TimeSpan(0, 0, 0);
				var fullDayTimeSpanEnd = new TimeSpan(23, 59, 0);
	            var startDateTime = new DateTime(message.RequestedDate.Year, message.RequestedDate.Month,
		            message.RequestedDate.Day, fullDayTimeSpanStart.Hours, fullDayTimeSpanStart.Minutes,
		            fullDayTimeSpanStart.Seconds);
	            var endDateTime = new DateTime(message.RequestedDate.Year, message.RequestedDate.Month,
					message.RequestedDate.Day, fullDayTimeSpanEnd.Hours, fullDayTimeSpanEnd.Minutes,
					fullDayTimeSpanEnd.Seconds);

				var period = new DateTimePeriod(
						DateTime.SpecifyKind(
							TimeZoneHelper.ConvertToUtc(startDateTime, agentTimeZone),
							DateTimeKind.Utc),
						DateTime.SpecifyKind(
							TimeZoneHelper.ConvertToUtc(endDateTime, agentTimeZone),
							DateTimeKind.Utc));
	            var allowedAbsencesForReport = person.WorkflowControlSet.AllowedAbsencesForReport.ToList();
	            
	            var reportedAbsence =
		            allowedAbsencesForReport.Single(x => x.Id == message.AbsenceId);
	            var dateOnlyPeriod = period.ToDateOnlyPeriod(agentTimeZone);

	            IEnumerable<IAbsenceRequestValidator> validatorList = null;
	            IRequestApprovalService requestApprovalServiceScheduler = null;
	            var undoRedoContainer = new UndoRedoContainer(400);

	            if (person.WorkflowControlSet == null)
	            {
		            if (Logger.IsDebugEnabled)
		            {
			            Logger.DebugFormat(CultureInfo.CurrentCulture,
				            "No workflow control set defined for {0}, {1} (PersonId = {2}). The reported absence with Id = {3} will not be processed.",
				            person.EmploymentNumber, person.Name, message.AbsenceId);
		            }
	            }

				if (person.WorkflowControlSet != null)
	            {
					IOpenAbsenceRequestPeriodExtractor extractor =
						person.WorkflowControlSet.GetExtractorForAbsence(reportedAbsence);
					extractor.ViewpointDate = DateOnly.Today;

					var openPeriods = extractor.Projection.GetProjectedPeriods(dateOnlyPeriod,
						person.PermissionInformation.Culture());

					var mergedPeriod = _absenceRequestOpenPeriodMerger.Merge(openPeriods);

					validatorList = mergedPeriod.GetSelectedValidatorList();

					_process = mergedPeriod.AbsenceRequestProcess;

					LoadDataForResourceCalculation(validatorList, period, person);

					_schedulingResultStateHolder.Schedules.TakeSnapshot();
					_schedulingResultStateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);

		            //var alreadyAbsent = personAlreadyAbsentDuringRequestPeriod();
		            var allNewRules = NewBusinessRuleCollection.Minimum();
		            requestApprovalServiceScheduler = _factory.GetRequestApprovalService(allNewRules,
			            _scenarioRepository.
				            Current());
		            var brokenBusinessRules = requestApprovalServiceScheduler.ApproveAbsence(reportedAbsence, period, person);

		            if (Logger.IsDebugEnabled)
		            {
			            foreach (var brokenBusinessRule in brokenBusinessRules)
			            {
				            Logger.DebugFormat("A rule was broken: {0}", brokenBusinessRule.Message);
			            }

			            Logger.Debug("Simulated approving absence successfully");
		            }

		            if (_process.GetType() == typeof (GrantAbsenceRequest) )
		            {
						//if (Logger.IsDebugEnabled)
						//{
						//	Logger.DebugFormat(CultureInfo.CurrentCulture,
						//		"The person is already absent during the absence request period. {0}, {1} (PersonId = {2}). The reported absence with Id = {3} will not be processed.",
						//		_absenceRequest.Person.EmploymentNumber, _absenceRequest.Person.Name,
						//		_absenceRequest.Person.Id,
						//		message.AbsenceId);
						//}
		            }

		            HandleInvalidSchedule();
	            }

	            //Will issue a rollback for simulated schedule data
	            if (Logger.IsInfoEnabled)
	            {
		            Logger.InfoFormat("The following process will be used for absence report with absence ID: {0}: {1}", message.AbsenceId,
			            _process.GetType());
	            }

		            try
		            {
			            PersistScheduleChanges(person);
		            }
		            catch (ValidationException validationException)
		            {
			            Logger.Error("A validation error occurred. Review the error log. Processing cannot continue.",
				            validationException);
			            ClearStateHolder();
			            return;
		            }

				_updateScheduleProjectionReadModel.Execute(_schedulingResultStateHolder.Schedules[person], dateOnlyPeriod);

				unitOfWork.PersistAll();
            }
	        ClearStateHolder();
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

        private void ClearStateHolder()
        {
            _schedulingResultStateHolder.Dispose();
            _schedulingResultStateHolder = null;
        }

        private void LoadDataForResourceCalculation(IEnumerable<IAbsenceRequestValidator> validatorList, DateTimePeriod period, IPerson person)
        {
            var shouldLoadDataForResourceCalculation = validatorList != null && validatorList.Any(v => typeof(StaffingThresholdValidator) == v.GetType());
            if (shouldLoadDataForResourceCalculation)
            {
                DateTimePeriod periodForResourceCalc = period.ChangeStartTime(TimeSpan.FromDays(-1));
				_prereqLoader.Execute();
                _loadSchedulingStateHolderForResourceCalculation.Execute(_scenarioRepository.Current(),
                                                                         periodForResourceCalc,
                                                                         new List<IPerson> {person});
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Loaded schedules and data needed for resource calculation. (Period = {0})",
                                       periodForResourceCalc);
                }
            }
            else
            {
                DateTimePeriod periodForResourceCalc = period.ChangeStartTime(TimeSpan.FromDays(-1));
                _loadSchedulesForRequestWithoutResourceCalculation.Execute(_scenarioRepository.Current(),
                                                                         periodForResourceCalc,
                                                                         new List<IPerson> { person });
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Loaded schedules and data needed for absence request handling. (Period = {0})",
                                       periodForResourceCalc);
                }
            }
        }

        private bool loadDefaultScenario(NewAbsenceReportCreated message)
        {
            var defaultScenario = _scenarioRepository.Current();
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", defaultScenario.Description,
                                   defaultScenario.Id);
            }
            return true;
        }

        private delegate bool LoadDataAction(NewAbsenceReportCreated message);
    }
}
