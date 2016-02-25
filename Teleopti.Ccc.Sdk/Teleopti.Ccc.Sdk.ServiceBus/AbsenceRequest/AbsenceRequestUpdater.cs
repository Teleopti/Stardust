using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest
{
	public class AbsenceRequestUpdater : IAbsenceRequestUpdater
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestConsumer));

		private readonly DenyAbsenceRequest _denyAbsenceRequest = new DenyAbsenceRequest();
		private readonly PendingAbsenceRequest _pendingAbsenceRequest = new PendingAbsenceRequest();

		private readonly IPersonAbsenceAccountProvider _personAbsenceAccountProvider;
		private readonly IResourceCalculationPrerequisitesLoader _prereqLoader;
		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;

		private readonly IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
		private readonly IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
		private readonly IScheduleIsInvalidSpecification _scheduleIsInvalidSpecification;
		private readonly IAlreadyAbsentSpecification _alreadyAbsentSpecification;

		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;

		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;

		private readonly IPersonAccountUpdater _personAccountUpdater;

		private IProcessAbsenceRequest _process;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IToggleManager _toggleManager;

		public AbsenceRequestUpdater(IPersonAbsenceAccountProvider personAbsenceAccountProvider, IResourceCalculationPrerequisitesLoader prereqLoader, ICurrentScenario scenarioRepository, ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation, ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation, IRequestFactory requestFactory, IAlreadyAbsentSpecification alreadyAbsentSpecification, IScheduleIsInvalidSpecification scheduleIsInvalidSpecification, IPersonRequestCheckAuthorization authorization, IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification, IResourceOptimizationHelper resourceOptimizationHelper, IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification, IScheduleDifferenceSaver scheduleDictionarySaver, IPersonAccountUpdater personAccountUpdater, IToggleManager toggleManager)
		{
			_personAbsenceAccountProvider = personAbsenceAccountProvider;
			_prereqLoader = prereqLoader;
			_scenarioRepository = scenarioRepository;
			_loadSchedulingStateHolderForResourceCalculation = loadSchedulingStateHolderForResourceCalculation;
			_loadSchedulesForRequestWithoutResourceCalculation = loadSchedulesForRequestWithoutResourceCalculation;
			_requestFactory = requestFactory;
			_alreadyAbsentSpecification = alreadyAbsentSpecification;
			_scheduleIsInvalidSpecification = scheduleIsInvalidSpecification;
			_authorization = authorization;
			_budgetGroupHeadCountSpecification = budgetGroupHeadCountSpecification;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_budgetGroupAllowanceSpecification = budgetGroupAllowanceSpecification;
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_personAccountUpdater = personAccountUpdater;
			_toggleManager = toggleManager;
		}

		public bool UpdateAbsenceRequest(IPersonRequest personRequest, IAbsenceRequest absenceRequest, IUnitOfWork unitOfWork, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;

			IAbsenceRequestValidator[] validatorList = null;
			IPersonAccountBalanceCalculator personAccountBalanceCalculator = null;
			IRequestApprovalService requestApprovalServiceScheduler = null;
			IPersonAbsenceAccount affectedPersonAbsenceAccount = null;
			var agentTimeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

			var undoRedoContainer = new UndoRedoContainer(400);

			var workflowControlSet = absenceRequest.Person.WorkflowControlSet;
			if (workflowControlSet == null)
			{
				handleNoWorkflowControlSet(absenceRequest, personRequest);
			}
			else
			{
				if (_toggleManager.IsEnabled(Toggles.Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850))
				{
					updatePersonAccountBalancesForAbsence(unitOfWork, absenceRequest);
				}

				var allAccounts = _personAbsenceAccountProvider.Find(absenceRequest.Person);
				affectedPersonAbsenceAccount = allAccounts.Find(absenceRequest.Absence);

				var mergedPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod(absenceRequest);
				validatorList = mergedPeriod.GetSelectedValidatorList().ToArray();
				_process = mergedPeriod.AbsenceRequestProcess;

				loadDataForResourceCalculation(absenceRequest, validatorList);
				
				personAccountBalanceCalculator = getPersonAccountBalanceCalculator(affectedPersonAbsenceAccount, absenceRequest, personRequest, dateOnlyPeriod);

				setupUndoContainersAndTakeSnapshot(undoRedoContainer, allAccounts);
				
				checkIfPersonIsAlreadyAbsentDuringRequestPeriod(absenceRequest, personRequest);

				var businessRules = NewBusinessRuleCollection.Minimum();

				requestApprovalServiceScheduler = _requestFactory.GetRequestApprovalService(businessRules, _scenarioRepository.Current());
				simulateApproveAbsence(absenceRequest, requestApprovalServiceScheduler);

				//Will issue a rollback for simulated schedule data
				handleInvalidSchedule();
			}

			var requiredForProcessingAbsenceRequest = new RequiredForProcessingAbsenceRequest(
				undoRedoContainer,
				requestApprovalServiceScheduler,
				_authorization,
				()
							=>
				{
					if (affectedPersonAbsenceAccount != null)
						trackAccounts(affectedPersonAbsenceAccount, dateOnlyPeriod, absenceRequest);
				});

			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(
				_schedulingResultStateHolder,
				personAccountBalanceCalculator,
				_resourceOptimizationHelper,
				_budgetGroupAllowanceSpecification,
				_budgetGroupHeadCountSpecification);


			var returnValue = processRequest(personRequest, absenceRequest, requiredForProcessingAbsenceRequest, requiredForHandlingAbsenceRequest, validatorList);

			//Ugly fix to get the number updated for person account. Don't try this at home!
			if (personRequest.IsApproved)
			{
				if (affectedPersonAbsenceAccount != null)
				{
					trackAccounts (affectedPersonAbsenceAccount, dateOnlyPeriod, absenceRequest);
					unitOfWork.Merge (affectedPersonAbsenceAccount);
				}
			}

			return returnValue;

		}
		
		private void trackAccounts(IPersonAbsenceAccount personAbsenceAccount, DateOnlyPeriod period, IAbsenceRequest absenceRequest)
		{
			var scheduleRange = _schedulingResultStateHolder.Schedules[absenceRequest.Person];
			var rangePeriod = scheduleRange.Period.ToDateOnlyPeriod(absenceRequest.Person.PermissionInformation.DefaultTimeZone());

			foreach (IAccount account in personAbsenceAccount.Find(period))
			{
				var intersectingPeriod = account.Period().Intersection(rangePeriod);
				if (intersectingPeriod.HasValue)
				{
					IList<IScheduleDay> scheduleDays =
						new List<IScheduleDay>(scheduleRange.ScheduledDayCollection(intersectingPeriod.Value));

					if (logger.IsInfoEnabled)
					{
						logger.InfoFormat("Remaining before tracking: {0}", account.Remaining);
					}

					absenceRequest.Absence.Tracker.Track(account, absenceRequest.Absence, scheduleDays);
					
					if (logger.IsInfoEnabled)
					{
						logger.InfoFormat("Remaining after tracking: {0}", account.Remaining);
					}
				}
			}
		}
		

		private void setupUndoContainersAndTakeSnapshot(UndoRedoContainer undoRedoContainer, IEnumerable<IPersonAbsenceAccount> allAccounts)
		{
			_schedulingResultStateHolder.Schedules.TakeSnapshot();
			_schedulingResultStateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);

			foreach (var personAbsenceAccount in allAccounts)
			{
				undoRedoContainer.SaveState(personAbsenceAccount);
			}
		}

		private bool processRequest(IPersonRequest personRequest, IAbsenceRequest absenceRequest,
			RequiredForProcessingAbsenceRequest requiredForProcessingAbsenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest,
			IEnumerable<IAbsenceRequestValidator> validatorList)
		{
			if (logger.IsInfoEnabled)
			{
				logger.InfoFormat("The following process will be used for request {0}: {1}", personRequest.Id,
					_process.GetType());
			}

			_process.Process(null, absenceRequest,
				requiredForProcessingAbsenceRequest,
				requiredForHandlingAbsenceRequest,
				validatorList);

			if (personRequest.IsApproved)
			{
				try
				{
					persistScheduleChanges(absenceRequest.Person);
				}
				catch (ValidationException validationException)
				{
					logger.Error("A validation error occurred. Review the error log. Processing cannot continue.", validationException);
					return false;
				}

			}

			return true;
		}

		private void updatePersonAccountBalancesForAbsence(IUnitOfWork unitOfWork, IAbsenceRequest absenceRequest)
		{
			if (_personAccountUpdater.UpdateForAbsence(
				absenceRequest.Person,
				absenceRequest.Absence,
				new DateOnly(absenceRequest.Period.StartDateTime)))
			{
				unitOfWork.PersistAll();
			};
		}

		private void handleNoWorkflowControlSet(IAbsenceRequest absenceRequest, IPersonRequest personRequest)
		{
			denyAbsenceRequest(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoWorkflow",
				absenceRequest.Person.PermissionInformation.Culture()));

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(CultureInfo.CurrentCulture,
					"No workflow control set defined for {0}, {1} (PersonId = {2}). The request with Id = {3} will be denied.",
					absenceRequest.Person.EmploymentNumber, absenceRequest.Person.Name,
					absenceRequest.Person.Id, personRequest.Id);
			}
		}

		private void denyAbsenceRequest(string reasonResourceKey)
		{
			_denyAbsenceRequest.DenyReason = reasonResourceKey;
			_process = _denyAbsenceRequest;
		}


		private void loadDataForResourceCalculation(IAbsenceRequest absenceRequest, IEnumerable<IAbsenceRequestValidator> validatorList)
		{
			var shouldLoadDataForResourceCalculation = validatorList != null && validatorList.Any(v => typeof(StaffingThresholdValidator) == v.GetType());
			if (shouldLoadDataForResourceCalculation)
			{
				var periodForResourceCalc = absenceRequest.Period.ChangeStartTime(TimeSpan.FromDays(-1));
				_prereqLoader.Execute();
				_loadSchedulingStateHolderForResourceCalculation.Execute(_scenarioRepository.Current(),
																		 periodForResourceCalc,
																		 new List<IPerson> { absenceRequest.Person });
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Loaded schedules and data needed for resource calculation. (Period = {0})",
									   periodForResourceCalc);
				}
			}
			else
			{
				var periodForResourceCalc = absenceRequest.Period.ChangeStartTime(TimeSpan.FromDays(-1));
				_loadSchedulesForRequestWithoutResourceCalculation.Execute(_scenarioRepository.Current(),
																		 periodForResourceCalc,
																		 new List<IPerson> { absenceRequest.Person });
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Loaded schedules and data needed for absence request handling. (Period = {0})",
									   periodForResourceCalc);
				}
			}
		}


		private static void simulateApproveAbsence(IAbsenceRequest absenceRequest, IRequestApprovalService requestApprovalServiceScheduler)
		{
			var brokenBusinessRules = requestApprovalServiceScheduler.ApproveAbsence(absenceRequest.Absence, absenceRequest.Period, absenceRequest.Person);
			if (logger.IsDebugEnabled)
			{
				foreach (var brokenBusinessRule in brokenBusinessRules)
				{
					logger.DebugFormat("A rule was broken: {0}", brokenBusinessRule.Message);
				}

				logger.Debug("Simulated approving absence successfully");
			}
		}

		private IPersonAccountBalanceCalculator getPersonAccountBalanceCalculator(IPersonAbsenceAccount personAccount, IAbsenceRequest absenceRequest, IPersonRequest personRequest, DateOnlyPeriod dateOnlyPeriod)
		{
			IPersonAccountBalanceCalculator personAccountBalanceCalculator;

			if (personAccount == null)
			{
				if (absenceRequest.Absence.Tracker != null)
				{
					if (logger.IsDebugEnabled)
					{
						logger.DebugFormat(CultureInfo.CurrentCulture,
							"No person account defined for {0}, {1} (PersonId = {2}) with absence {4} for {5}. (Request Id = {3})",
							absenceRequest.Person.EmploymentNumber, absenceRequest.Person.Name,
							absenceRequest.Person.Id, personRequest.Id,
							absenceRequest.Absence.Description,
							dateOnlyPeriod.StartDate);
					}
				}
				personAccountBalanceCalculator = new EmptyPersonAccountBalanceCalculator(absenceRequest.Absence);
			}
			else
			{
				trackAccounts (personAccount, dateOnlyPeriod, absenceRequest);

				//We must have the current and all after...
				var affectedAccounts = personAccount.Find(new DateOnlyPeriod(dateOnlyPeriod.StartDate, DateOnly.MaxValue));

				personAccountBalanceCalculator = new PersonAccountBalanceCalculator(affectedAccounts);
			}

			return personAccountBalanceCalculator;
		}

		private void checkIfPersonIsAlreadyAbsentDuringRequestPeriod(IAbsenceRequest absenceRequest, IPersonRequest personRequest)
		{
			var alreadyAbsent = personAlreadyAbsentDuringRequestPeriod(absenceRequest);

			if (_process.GetType() == typeof(GrantAbsenceRequest) && alreadyAbsent)
			{
				denyAbsenceRequest(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonAlreadyAbsent",
					absenceRequest.Person.PermissionInformation.Culture()));

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat(CultureInfo.CurrentCulture,
						"The person is already absent during the absence request period. {0}, {1} (PersonId = {2}). The request with Id = {3} will be denied.",
						absenceRequest.Person.EmploymentNumber, absenceRequest.Person.Name,
						absenceRequest.Person.Id,
						personRequest.Id);
				}
			}
		}

		private bool personAlreadyAbsentDuringRequestPeriod(IAbsenceRequest absenceRequest)
		{
			return _alreadyAbsentSpecification.IsSatisfiedBy(absenceRequest);
		}


		private void handleInvalidSchedule()
		{
			if (_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder))
			{
				if (_process.GetType() != typeof(DenyAbsenceRequest))
				{
					_process = _pendingAbsenceRequest;
				}
			}
		}

		private void persistScheduleChanges(IPerson person)
		{
			_scheduleDictionarySaver.SaveChanges(_schedulingResultStateHolder.Schedules.DifferenceSinceSnapshot(), (IUnvalidatedScheduleRangeUpdate)_schedulingResultStateHolder.Schedules[person]);
		}


	}
}