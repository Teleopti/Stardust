using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestUpdater : IAbsenceRequestUpdater
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AbsenceRequestUpdater));

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
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IPersonAccountUpdater _personAccountUpdater;
		private IProcessAbsenceRequest _process;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IToggleManager _toggleManager;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;

		public AbsenceRequestUpdater(IPersonAbsenceAccountProvider personAbsenceAccountProvider,
			IResourceCalculationPrerequisitesLoader prereqLoader, ICurrentScenario scenarioRepository,
			ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation,
			IRequestFactory requestFactory, IAlreadyAbsentSpecification alreadyAbsentSpecification,
			IScheduleIsInvalidSpecification scheduleIsInvalidSpecification, IPersonRequestCheckAuthorization authorization,
			IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification,
			IResourceCalculation resourceOptimizationHelper,
			IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification,
			IScheduleDifferenceSaver scheduleDictionarySaver, IPersonAccountUpdater personAccountUpdater,
			IToggleManager toggleManager, IAbsenceRequestValidatorProvider absenceRequestValidatorProvider)
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
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
		}

		public bool UpdateAbsenceRequest(IPersonRequest personRequest, IAbsenceRequest absenceRequest, IUnitOfWork unitOfWork,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var workflowControlSet = absenceRequest.Person.WorkflowControlSet;
			var mergedPeriod = workflowControlSet?.GetMergedAbsenceRequestOpenPeriod(absenceRequest);
			var validatorList = mergedPeriod != null ? _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod) : null;
			var process = mergedPeriod?.AbsenceRequestProcess;
			return processAbsenceRequest(personRequest, absenceRequest, unitOfWork, schedulingResultStateHolder, process, validatorList);
		}

		private bool processAbsenceRequest(IPersonRequest personRequest, IAbsenceRequest absenceRequest, IUnitOfWork unitOfWork,
			ISchedulingResultStateHolder schedulingResultStateHolder, IProcessAbsenceRequest process, IEnumerable<IAbsenceRequestValidator> validatorList)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;

			IPersonAccountBalanceCalculator personAccountBalanceCalculator = null;
			IRequestApprovalService absenceRequestApprovalService = null;
			IPersonAbsenceAccount affectedPersonAbsenceAccount = null;
			var agentTimeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

			var undoRedoContainer = new UndoRedoContainer();

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

				_process = process;

				loadDataForResourceCalculation(absenceRequest, validatorList);

				personAccountBalanceCalculator = getPersonAccountBalanceCalculator(affectedPersonAbsenceAccount, absenceRequest,
					personRequest, dateOnlyPeriod);

				setupUndoContainersAndTakeSnapshot(undoRedoContainer, allAccounts);

				checkIfPersonIsAlreadyAbsentDuringRequestPeriod(absenceRequest, personRequest);

				var businessRules = NewBusinessRuleCollection.Minimum();

				absenceRequestApprovalService = _requestFactory.GetRequestApprovalService(businessRules,
					_scenarioRepository.Current(), schedulingResultStateHolder, personRequest);
				simulateApproveAbsence(absenceRequest, absenceRequestApprovalService);

				//Will issue a rollback for simulated schedule data
				handleInvalidSchedule();
			}

			var requiredForProcessingAbsenceRequest = new RequiredForProcessingAbsenceRequest(
				undoRedoContainer,
				absenceRequestApprovalService,
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


			var returnValue = processRequest(personRequest, absenceRequest, requiredForProcessingAbsenceRequest,
				requiredForHandlingAbsenceRequest, validatorList);

			//Ugly fix to get the number updated for person account. Don't try this at home!
			if (personRequest.IsApproved)
			{
				if (affectedPersonAbsenceAccount != null)
				{
					trackAccounts(affectedPersonAbsenceAccount, dateOnlyPeriod, absenceRequest);
					unitOfWork.Merge(affectedPersonAbsenceAccount);
				}

				var approvedPersonAbsence = ((AbsenceRequestApprovalService)absenceRequestApprovalService).GetApprovedPersonAbsence();
				approvedPersonAbsence?.IntradayAbsence(personRequest.Person, new TrackedCommandInfo
				{
					OperatedPersonId = personRequest.Person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				});
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

			_process.Process(absenceRequest,
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
			}
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

		private void denyAbsenceRequest(string reasonResourceKey, bool alreadyAbsence = false)
		{
			_denyAbsenceRequest.DenyReason = reasonResourceKey;
			_denyAbsenceRequest.DenyOption = alreadyAbsence ? PersonRequestDenyOption.AlreadyAbsence : PersonRequestDenyOption.None;
			_process = _denyAbsenceRequest;
		}


		private void loadDataForResourceCalculation(IAbsenceRequest absenceRequest, IEnumerable<IAbsenceRequestValidator> validatorList)
		{
			var shouldLoadDataForResourceCalculation = validatorList != null && validatorList.Any(v => v is StaffingThresholdValidator);
			if (shouldLoadDataForResourceCalculation)
			{
				var periodForResourceCalc = absenceRequest.Period.ChangeStartTime(TimeSpan.FromDays(-1));
				_prereqLoader.Execute();
				_loadSchedulingStateHolderForResourceCalculation.Execute(_scenarioRepository.Current(),
																		 periodForResourceCalc,
																		 new List<IPerson> { absenceRequest.Person }, _schedulingResultStateHolder);
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
																		 new List<IPerson> { absenceRequest.Person }, _schedulingResultStateHolder);
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Loaded schedules and data needed for absence request handling. (Period = {0})",
										periodForResourceCalc);
				}
			}
		}


		private static void simulateApproveAbsence(IAbsenceRequest absenceRequest, IRequestApprovalService absenceRequestApprovalService)
		{
			var brokenBusinessRules = absenceRequestApprovalService.Approve(absenceRequest);
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
				trackAccounts(personAccount, dateOnlyPeriod, absenceRequest);

				//We must have the current and all after...
				var affectedAccounts = personAccount.Find(new DateOnlyPeriod(dateOnlyPeriod.StartDate, DateOnly.MaxValue));

				personAccountBalanceCalculator = new PersonAccountBalanceCalculator(affectedAccounts);
			}

			return personAccountBalanceCalculator;
		}

		private void checkIfPersonIsAlreadyAbsentDuringRequestPeriod(IAbsenceRequest absenceRequest,
			IPersonRequest personRequest)
		{
			var alreadyAbsent = personAlreadyAbsentDuringRequestPeriod(absenceRequest);
			var requiredCheckAlreadyAbsent = _process.GetType() == typeof(GrantAbsenceRequest) ||
											 _process.GetType() == typeof(ApproveAbsenceRequestWithValidators);
			if (requiredCheckAlreadyAbsent && alreadyAbsent)
			{
				denyAbsenceRequest(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonAlreadyAbsent",
					absenceRequest.Person.PermissionInformation.Culture()), true);

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
			return
				_alreadyAbsentSpecification.IsSatisfiedBy(new AbsenceRequstAndSchedules
				{
					AbsenceRequest = absenceRequest,
					SchedulingResultStateHolder = _schedulingResultStateHolder
				});
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
			_scheduleDictionarySaver.SaveChanges(
				_schedulingResultStateHolder.Schedules[person].DifferenceSinceSnapshot(
					new DifferenceEntityCollectionService<IPersistableScheduleData>()),
				(IUnvalidatedScheduleRangeUpdate) _schedulingResultStateHolder.Schedules[person]);
		}
	}
}