using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class MultiAbsenceRequestsUpdater
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(MultiAbsenceRequestsUpdater));

		private readonly DenyAbsenceRequest _denyAbsenceRequest = new DenyAbsenceRequest();
		private readonly PendingAbsenceRequest _pendingAbsenceRequest = new PendingAbsenceRequest();

		private readonly IResourceCalculationPrerequisitesLoader _prereqLoader;
		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private readonly IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
		private readonly IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
		private readonly IScheduleIsInvalidSpecification _scheduleIsInvalidSpecification;
		private readonly IAlreadyAbsentSpecification _alreadyAbsentSpecification;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private IProcessAbsenceRequest _process;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICommandDispatcher _commandDispatcher;

		public MultiAbsenceRequestsUpdater(IResourceCalculationPrerequisitesLoader prereqLoader, 
			ICurrentScenario scenarioRepository,
			ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation,
			IRequestFactory requestFactory, 
			IAlreadyAbsentSpecification alreadyAbsentSpecification,
			IScheduleIsInvalidSpecification scheduleIsInvalidSpecification, 
			IPersonRequestCheckAuthorization authorization,
			IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
			ICommandDispatcher commandDispatcher)
		{
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
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_commandDispatcher = commandDispatcher;
		}

		public void UpdateAbsenceRequest(List<IPersonRequest> personRequests,
										 ISchedulingResultStateHolder schedulingResultStateHolder, IProcessAbsenceRequest process, IEnumerable<IAbsenceRequestValidator> validators)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;

			var aggregatedValidatorList = new HashSet<IAbsenceRequestValidator>();

			foreach (var personRequest in personRequests)
			{
				var person = personRequest.Person;
				if (person.WorkflowControlSet != null)
				{
					var mergedPeriod = person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((AbsenceRequest) personRequest.Request);
					aggregatedValidatorList.UnionWith(validators ?? mergedPeriod.GetSelectedValidatorList());
				}
			}
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				loadDataForResourceCalculation(personRequests, aggregatedValidatorList);

				foreach (var personRequest in personRequests)
				{
					var absenceRequest = personRequest.Request as IAbsenceRequest;

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
						IPersonAccountCollection allAccounts;
						if(! _schedulingResultStateHolder.AllPersonAccounts.TryGetValue(absenceRequest.Person, out allAccounts ))
							allAccounts = new PersonAccountCollection(absenceRequest.Person);

						affectedPersonAbsenceAccount = allAccounts.Find(absenceRequest.Absence);
						var currentScenario = _scenarioRepository.Current();

						var mergedPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod(absenceRequest);
						validatorList = (validators ?? mergedPeriod.GetSelectedValidatorList()).ToArray();
						_process = process ?? mergedPeriod.AbsenceRequestProcess;


						personAccountBalanceCalculator = getPersonAccountBalanceCalculator(affectedPersonAbsenceAccount, absenceRequest,
																						   personRequest, dateOnlyPeriod);

						setupUndoContainersAndTakeSnapshot(undoRedoContainer, allAccounts);

						checkIfPersonIsAlreadyAbsentDuringRequestPeriod(absenceRequest, personRequest);

						var businessRules = NewBusinessRuleCollection.Minimum();

						requestApprovalServiceScheduler = _requestFactory.GetRequestApprovalService(businessRules, currentScenario, schedulingResultStateHolder);

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


					processRequest(personRequest, absenceRequest, requiredForProcessingAbsenceRequest,
								   requiredForHandlingAbsenceRequest, validatorList);
				}
			}

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				foreach (var personRequest in personRequests)
				{
					IRequestCommand command;

					if (personRequest.IsApproved)
					{
						command = new ApproveRequestCommand()
						{
							PersonRequestId = personRequest.Id.GetValueOrDefault(),
						};
					}
					else
					{
						command = new DenyRequestCommand()
						{
							PersonRequestId = personRequest.Id.GetValueOrDefault(),
							DenyReason = personRequest.DenyReason
						};
					}
					_commandDispatcher.Execute(command);

					if (command.ErrorMessages != null)
					{
						logger.Warn(command.ErrorMessages);
					}

					try
					{
						uow.PersistAll();
					}
					catch (OptimisticLockException)
					{

					}
				}
			}
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

			return true;
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
			_denyAbsenceRequest.AlreadyAbsence = alreadyAbsence;
			_process = _denyAbsenceRequest;
		}


		private void loadDataForResourceCalculation(List<IPersonRequest> personRequests, IEnumerable<IAbsenceRequestValidator> validatorList)
		{
			var shouldLoadDataForResourceCalculation = validatorList != null && validatorList.Any(v => typeof(StaffingThresholdValidator) == v.GetType());

			var totalPeriod = personRequests.First().Request.Period;
			var persons = new HashSet<IPerson>();

			foreach (var personRequest in personRequests)
			{
				if (totalPeriod.StartDateTime > personRequest.Request.Period.StartDateTime)
				{
					totalPeriod = new DateTimePeriod(personRequest.Request.Period.StartDateTime, totalPeriod.EndDateTime);
				}
				if (totalPeriod.EndDateTime < personRequest.Request.Period.EndDateTime)
				{
					totalPeriod = new DateTimePeriod(totalPeriod.StartDateTime, personRequest.Request.Period.EndDateTime);
				}
				persons.Add(personRequest.Person);
			}
			totalPeriod.ChangeStartTime(TimeSpan.FromDays(-1));

			if (shouldLoadDataForResourceCalculation)
			{
				_prereqLoader.Execute();
				_loadSchedulingStateHolderForResourceCalculation.Execute(_scenarioRepository.Current(),
																		 totalPeriod,
																		 persons, _schedulingResultStateHolder);
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Loaded schedules and data needed for resource calculation. (Period = {0})",
									   totalPeriod);
				}
			}
			else
			{
				_loadSchedulesForRequestWithoutResourceCalculation.Execute(_scenarioRepository.Current(),
																		   totalPeriod,
																		   persons, _schedulingResultStateHolder);
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Loaded schedules and data needed for absence request handling. (Period = {0})",
									   totalPeriod);
				}
			}
		}

		private static void simulateApproveAbsence(IAbsenceRequest absenceRequest, IRequestApprovalService requestApprovalServiceScheduler)
		{
			var personRequest = absenceRequest.Parent as IPersonRequest;
			var brokenBusinessRules = requestApprovalServiceScheduler.ApproveAbsence(absenceRequest.Absence, absenceRequest.Period, absenceRequest.Person, personRequest);
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

	}
}