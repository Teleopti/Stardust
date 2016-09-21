using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
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
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class MultiAbsenceRequestsUpdater : IMultiAbsenceRequestsUpdater
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
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IStardustJobFeedback _feedback;
		private readonly ArrangeRequestsByProcessOrder _arrangeRequestsByProcessOrder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

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
			ICommandDispatcher commandDispatcher,
			IStardustJobFeedback feedback, 
			ArrangeRequestsByProcessOrder arrangeRequestsByProcessOrder, 
			IScheduleDayChangeCallback scheduleDayChangeCallback, 
			ISchedulingResultStateHolder schedulingResultStateHolder)
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
			_feedback = feedback;
			_arrangeRequestsByProcessOrder = arrangeRequestsByProcessOrder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void UpdateAbsenceRequest(List<IPersonRequest> personRequests)
		{
			var aggregatedValidatorList = new HashSet<IAbsenceRequestValidator>();
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				foreach (var personRequest in personRequests)
				{
					var person = personRequest.Person;
					if (person.WorkflowControlSet != null)
					{
						var mergedPeriod = person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((AbsenceRequest) personRequest.Request);
						aggregatedValidatorList.UnionWith(mergedPeriod.GetSelectedValidatorList());
					}
				}
				loadDataForResourceCalculation(personRequests, aggregatedValidatorList);
				_feedback.SendProgress?.Invoke("Done loading data for resource calculation!");

				var seniority = _arrangeRequestsByProcessOrder.GetRequestsSortedBySeniority(personRequests);
				var firstComeFirstServe = _arrangeRequestsByProcessOrder.GetRequestsSortedByDate(personRequests);
				processOrderList(seniority, _schedulingResultStateHolder);
				processOrderList(firstComeFirstServe, _schedulingResultStateHolder);

			}
			foreach (var personRequest in personRequests)
			{
				var count = 0;
				while (count < 3)
				{
					try
					{
						using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
						{
							sendRequestCommand(personRequest);
							uow.PersistAll();
							break;
						}
					}
					catch (OptimisticLockException)
					{
						count++;
					}
				}
				if (count >= 3)
				{
					logger.Error("Optimistic lock when persisting an absence request! Number of retries: " + count);
				}
				_feedback.SendProgress?.Invoke($"Persisted request {personRequest.Id}.");
			}
		}

		private void processOrderList(IList<IPersonRequest> requests, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			foreach (var personRequest in requests)
			{
				var absenceRequest = personRequest.Request as IAbsenceRequest;

				IProcessAbsenceRequest process;
				IEnumerable<IAbsenceRequestValidator> validatorList = null;
				IPersonAccountBalanceCalculator personAccountBalanceCalculator = null;
				IRequestApprovalService requestApprovalServiceScheduler = null;
				IPersonAbsenceAccount affectedPersonAbsenceAccount = null;
				var agentTimeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
				var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

				var undoRedoContainer = new UndoRedoContainer(_scheduleDayChangeCallback, 400);

				var workflowControlSet = absenceRequest.Person.WorkflowControlSet;
				if (workflowControlSet == null)
				{
					process = handleNoWorkflowControlSet(absenceRequest);
				}
				else
				{
					IPersonAccountCollection allAccounts;
					if (!_schedulingResultStateHolder.AllPersonAccounts.TryGetValue(absenceRequest.Person, out allAccounts))
						allAccounts = new PersonAccountCollection(absenceRequest.Person);

					affectedPersonAbsenceAccount = allAccounts.Find(absenceRequest.Absence);
					var currentScenario = _scenarioRepository.Current();

					var mergedPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod(absenceRequest);
					validatorList = mergedPeriod.GetSelectedValidatorList();
					process = mergedPeriod.AbsenceRequestProcess;


					personAccountBalanceCalculator = getPersonAccountBalanceCalculator(affectedPersonAbsenceAccount, absenceRequest, dateOnlyPeriod);

					setupUndoContainersAndTakeSnapshot(undoRedoContainer, allAccounts);

					process = checkIfPersonIsAlreadyAbsentDuringRequestPeriod(absenceRequest, process);

					var businessRules = NewBusinessRuleCollection.Minimum();

					requestApprovalServiceScheduler = _requestFactory.GetRequestApprovalService(businessRules, currentScenario, schedulingResultStateHolder);

					simulateApproveAbsence(absenceRequest, requestApprovalServiceScheduler);

					//Will issue a rollback for simulated schedule data
					process = handleInvalidSchedule(process);
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

				process.Process(null, absenceRequest,
								requiredForProcessingAbsenceRequest,
								requiredForHandlingAbsenceRequest,
								validatorList);

				_feedback.SendProgress?.Invoke($"Processed request {personRequest.Id} ({personRequest.Person.Name}, {personRequest.Request.Period}).");
			}
		}

		private void sendRequestCommand(IPersonRequest personRequest)
		{
			if (personRequest.IsApproved || personRequest.IsDenied)
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
						DenyReason = personRequest.DenyReason,
						IsAlreadyAbsent = personRequest.IsAlreadyAbsent
					};
				}
				_commandDispatcher.Execute(command);

				if (command.ErrorMessages != null)
				{
					logger.Warn(command.ErrorMessages);
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

		private IProcessAbsenceRequest handleNoWorkflowControlSet(IAbsenceRequest absenceRequest)
		{
			return denyAbsenceRequest(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoWorkflow",
				absenceRequest.Person.PermissionInformation.Culture()));
		}

		private IProcessAbsenceRequest denyAbsenceRequest(string reasonResourceKey, bool alreadyAbsence = false)
		{
			_denyAbsenceRequest.DenyReason = reasonResourceKey;
			_denyAbsenceRequest.AlreadyAbsence = alreadyAbsence;
			return _denyAbsenceRequest;
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
			}
			else
			{
				_loadSchedulesForRequestWithoutResourceCalculation.Execute(_scenarioRepository.Current(),
																		   totalPeriod,
																		   persons, _schedulingResultStateHolder);
			}
		}

		private static void simulateApproveAbsence(IAbsenceRequest absenceRequest, IRequestApprovalService requestApprovalServiceScheduler)
		{
			var personRequest = absenceRequest.Parent as IPersonRequest;
			requestApprovalServiceScheduler.ApproveAbsence(absenceRequest.Absence, absenceRequest.Period, absenceRequest.Person, personRequest);
		}


		private IPersonAccountBalanceCalculator getPersonAccountBalanceCalculator(IPersonAbsenceAccount personAccount, IAbsenceRequest absenceRequest, DateOnlyPeriod dateOnlyPeriod)
		{
			IPersonAccountBalanceCalculator personAccountBalanceCalculator;

			if (personAccount == null)
			{
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

		private IProcessAbsenceRequest checkIfPersonIsAlreadyAbsentDuringRequestPeriod(IAbsenceRequest absenceRequest, IProcessAbsenceRequest process)
		{
			var alreadyAbsent = personAlreadyAbsentDuringRequestPeriod(absenceRequest);
			var requiredCheckAlreadyAbsent = process.GetType() == typeof(GrantAbsenceRequest) ||
											 process.GetType() == typeof(ApproveAbsenceRequestWithValidators);
			if (requiredCheckAlreadyAbsent && alreadyAbsent)
			{
				process =  denyAbsenceRequest(UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonAlreadyAbsent",
					absenceRequest.Person.PermissionInformation.Culture()), true);
			}
			return process;
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


		private IProcessAbsenceRequest handleInvalidSchedule(IProcessAbsenceRequest process)
		{
			if (_scheduleIsInvalidSpecification.IsSatisfiedBy(_schedulingResultStateHolder))
			{
				if (process.GetType() != typeof(DenyAbsenceRequest))
				{
					process = _pendingAbsenceRequest;
				}
			}
			return process;
		}

	}
}