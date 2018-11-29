using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class MultiAbsenceRequestsUpdater : IMultiAbsenceRequestsUpdater
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(MultiAbsenceRequestsUpdater));
		private static readonly ILog requestLogger = LogManager.GetLogger("Teleopti.Requests");

		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private readonly IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
		private readonly IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
		private readonly IAlreadyAbsentSpecification _alreadyAbsentSpecification;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly BudgetGroupState _budgetGroupState;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IStardustJobFeedback _feedback;
		private readonly ArrangeRequestsByProcessOrder _arrangeRequestsByProcessOrder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;


		private IDictionary<Guid, IEnumerable<IAbsenceRequestValidator>> _absenceRequestValidators;

		public MultiAbsenceRequestsUpdater(
			ICurrentScenario scenarioRepository,
			ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation,
			IRequestFactory requestFactory, 
			IAlreadyAbsentSpecification alreadyAbsentSpecification,
			IPersonRequestCheckAuthorization authorization,
			IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification,
			IResourceCalculation resourceOptimizationHelper,
			IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
			ICommandDispatcher commandDispatcher,
			IStardustJobFeedback feedback, 
			ArrangeRequestsByProcessOrder arrangeRequestsByProcessOrder, 
			IScheduleDayChangeCallback scheduleDayChangeCallback, 
			ISchedulingResultStateHolder schedulingResultStateHolder, 
			BudgetGroupState budgetGroupState,
			CascadingResourceCalculationContextFactory resourceCalculationContextFactory, 
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, 
			IPersonRepository personRepository, 
			ISkillRepository skillRepository, 
			IContractRepository contractRepository, 
			IPartTimePercentageRepository partTimePercentageRepository, 
			IContractScheduleRepository contractScheduleRepository,
			ISkillTypeRepository skillTypeRepository, 
			IActivityRepository activityRepository, 
			IAbsenceRepository absenceRepository, 
			IPersonRequestRepository personRequestRepository, 
			IDayOffTemplateRepository dayOffTemplateRepository)
		{
			_scenarioRepository = scenarioRepository;
			_loadSchedulingStateHolderForResourceCalculation = loadSchedulingStateHolderForResourceCalculation;
			_loadSchedulesForRequestWithoutResourceCalculation = loadSchedulesForRequestWithoutResourceCalculation;
			_requestFactory = requestFactory;
			_alreadyAbsentSpecification = alreadyAbsentSpecification;
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
			_budgetGroupState = budgetGroupState;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_personRepository = personRepository;
			_skillRepository = skillRepository;
			_contractRepository = contractRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_skillTypeRepository = skillTypeRepository;
			_activityRepository = activityRepository;
			_absenceRepository = absenceRepository;
			_personRequestRepository = personRequestRepository;
			_dayOffTemplateRepository = dayOffTemplateRepository;
		}

		public void UpdateAbsenceRequest(IList<Guid> personRequestsIds)
		{
			if (!personRequestsIds.Any()) return;
			try
			{
				var aggregatedValidatorList = new HashSet<IAbsenceRequestValidator>();
				IList<IPersonRequest> personRequests;
				var stopwatch = new Stopwatch();

				using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					stopwatch.Restart();
					preloadData();
					personRequests = _personRequestRepository.Find(personRequestsIds).Where(x => x.IsPending || x.IsWaitlisted).ToList();
					_personRepository.FindPeople(personRequests.Select(x => x.Person.Id.GetValueOrDefault()));
					var currentScenario = _scenarioRepository.Current();
					stopwatch.Stop();
					_feedback.SendProgress($"Done preloading data! It took {stopwatch.Elapsed}");


					foreach (var personRequest in personRequests)
					{
						var person = personRequest.Person;
						if (person.WorkflowControlSet != null)
						{
							var mergedPeriod =
								person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((AbsenceRequest) personRequest.Request);
							aggregatedValidatorList.UnionWith(mergedPeriod.GetSelectedValidatorList());
							if (_absenceRequestValidators != null &&
								_absenceRequestValidators.ContainsKey(personRequest.Id.GetValueOrDefault()))
							{
								aggregatedValidatorList.UnionWith(_absenceRequestValidators[personRequest.Id.GetValueOrDefault()]);
							}
						}
					}
					
					stopwatch.Restart();
					loadDataForResourceCalculation(personRequests, aggregatedValidatorList);
					stopwatch.Stop();
					_feedback.SendProgress($"Done loading data for resource calculation! It took {stopwatch.Elapsed}");

					var seniority = _arrangeRequestsByProcessOrder.GetRequestsSortedBySeniority(personRequests);
					var firstComeFirstServe = _arrangeRequestsByProcessOrder.GetRequestsSortedByDate(personRequests);

					stopwatch.Restart();
					using (_resourceCalculationContextFactory.Create(_schedulingResultStateHolder, false, _schedulingResultStateHolder.Schedules.Period.LongVisibleDateOnlyPeriod()))
					{
						stopwatch.Stop();
						_feedback.SendProgress($"Done _resourceCalculationContextFactory.Create(..)! It took {stopwatch.Elapsed}");
						processOrderList(seniority, currentScenario);
						processOrderList(firstComeFirstServe, currentScenario);
					}
				}
				foreach (var personRequest in personRequests)
				{
					sendCommandWithRetries(personRequest);
				}
			}
			catch (Exception exp)
			{
				if (exp.IsSqlDeadlock())
				{
					_feedback.SendProgress("The bulk for absence requests cannot be processed due to a deadlock " + exp);
					throw;
				}
				_feedback.SendProgress("The bulk for absence requests failed! " + exp);
				logger.Error("The bulk for absence requests failed! ", exp);
				using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var personRequests = _personRequestRepository.Find(personRequestsIds);
					_personRepository.FindPeople(personRequests.Select(x => x.Person.Id.GetValueOrDefault()));
					foreach (var personRequest in personRequests)
					{
						denyDueToTechnicalIssues(personRequest);
					}
					uow.PersistAll();
				}
				throw;
			}
		}

		public void UpdateAbsenceRequest(IList<Guid> personRequests, IDictionary<Guid, IEnumerable<IAbsenceRequestValidator>> absenceRequestValidators)
		{
			_absenceRequestValidators = absenceRequestValidators;
			UpdateAbsenceRequest(personRequests);
		}

		private void sendCommandWithRetries(IPersonRequest personRequest)
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
				string message = $"Optimistic lock when persisting request ({personRequest.Id.GetValueOrDefault()})! Number of retries: {count}";
				logger.Warn(message);
				_feedback.SendProgress(message);
			}
		}

		private void preloadData()
		{
			_contractRepository.LoadAll();
			_skillTypeRepository.LoadAll();
			_partTimePercentageRepository.LoadAll();
			_contractScheduleRepository.LoadAllAggregate();
			_activityRepository.LoadAll();
			_absenceRepository.LoadAll();
			_dayOffTemplateRepository.LoadAll();
			_skillRepository.LoadAllSkills();
		}

		private void processOrderList(IList<IPersonRequest> requests, IScenario currentScenario)
		{
			var stopwatch = new Stopwatch();
			foreach (var personRequest in requests)
			{
				
					requestLogger.Debug($"Start processing absence request for {personRequest.Person.Name} for period {personRequest.Request.Period}");
					var absenceRequest = personRequest.Request as IAbsenceRequest;
					if (absenceRequest == null) continue;
				try
				{
					var agentTimeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
					var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

					var undoRedoContainer = new UndoRedoWithScheduleCallbackContainer(_scheduleDayChangeCallback);

					var workflowControlSet = absenceRequest.Person.WorkflowControlSet;

					// TODO skip loading if no personAccount Check
					IPersonAccountCollection allAccounts;
					if (!_schedulingResultStateHolder.AllPersonAccounts.TryGetValue(absenceRequest.Person, out allAccounts))
						allAccounts = new PersonAccountCollection(absenceRequest.Person);

					var affectedPersonAbsenceAccount = allAccounts.Find(absenceRequest.Absence);
					var absenceRequestApprovalService = _requestFactory.GetRequestApprovalService(NewBusinessRuleCollection.Minimum(), currentScenario, _schedulingResultStateHolder, personRequest);

					var mergedPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod(absenceRequest);
					var validatorList = getValidatorList(personRequest.Id.Value, mergedPeriod);
					var processAbsenceRequest = getAbsenceRequestProcess(personRequest.Id.Value, mergedPeriod);
					var personAccountBalanceCalculator = getPersonAccountBalanceCalculator(affectedPersonAbsenceAccount, absenceRequest, dateOnlyPeriod);

					if (processAbsenceRequest.GetType() != typeof(DenyAbsenceRequest))
					{
						setupUndoContainersAndTakeSnapshot(undoRedoContainer, allAccounts);
						processAbsenceRequest = checkIfPersonIsAlreadyAbsentDuringRequestPeriod(absenceRequest, processAbsenceRequest);

						if (processAbsenceRequest.GetType() != typeof(DenyAbsenceRequest))
						{
							simulateApproveAbsence(personRequest.Request, absenceRequestApprovalService);
						}
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
						_budgetGroupState,
						personAccountBalanceCalculator,
						_resourceOptimizationHelper,
						_budgetGroupAllowanceSpecification,
						_budgetGroupHeadCountSpecification);

					stopwatch.Restart();
					processAbsenceRequest.Process(absenceRequest,
												  requiredForProcessingAbsenceRequest,
												  requiredForHandlingAbsenceRequest,
												  validatorList);
					stopwatch.Stop();
				}
				catch (Exception exp)
				{
					_feedback.SendProgress($"Absence Request failed! {personRequest.Id.GetValueOrDefault()}" + exp);
					logger.Error($"Absence Request failed! {personRequest.Id.GetValueOrDefault()}", exp);
					throw;
				}
			}
		}

		private void denyDueToTechnicalIssues(IPersonRequest personRequest)
		{
			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = Resources.ResourceManager.GetString(nameof(Resources.DenyReasonTechnicalIssues), personRequest.Person.PermissionInformation.Culture()),
				DenyOption = PersonRequestDenyOption.None
			};

			_commandDispatcher.Execute(command);
		}

		private IProcessAbsenceRequest getAbsenceRequestProcess(Guid personRequestId, IAbsenceRequestOpenPeriod mergedPeriod)
		{
			return useSpecificValidators(personRequestId)
				? new ApproveAbsenceRequestWithValidators()
				: mergedPeriod.AbsenceRequestProcess;
		}

		private IEnumerable<IAbsenceRequestValidator> getValidatorList(Guid personRequestId,
			IAbsenceRequestOpenPeriod mergedPeriod)
		{
			return useSpecificValidators(personRequestId)
				? _absenceRequestValidators[personRequestId]
				: _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);
		}

		private bool useSpecificValidators(Guid personRequestsId)
		{
			return _absenceRequestValidators != null && _absenceRequestValidators.ContainsKey(personRequestsId)
				   && _absenceRequestValidators[personRequestsId].Any();
		}

		private void sendRequestCommand(IPersonRequest personRequest)
		{
			var stopwatch = new Stopwatch();
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
					var denyOption = PersonRequestDenyOption.None;
					if (personRequest.IsAlreadyAbsent)
						denyOption = PersonRequestDenyOption.AlreadyAbsence;
					else if (personRequest.IsExpired)
						denyOption = PersonRequestDenyOption.RequestExpired;
					else if (personRequest.InsufficientPersonAccount)
						denyOption = PersonRequestDenyOption.InsufficientPersonAccount;

					command = new DenyRequestCommand()
					{
						PersonRequestId = personRequest.Id.GetValueOrDefault(),
						DenyReason = personRequest.DenyReason,
						DenyOption = denyOption
					};
				}
				stopwatch.Restart();
				_commandDispatcher.Execute(command);
				stopwatch.Stop();
				if (command.ErrorMessages.Count > 0)
				{
					logger.Warn(command.ErrorMessages);
					foreach (var error in command.ErrorMessages)
					{
						_feedback.SendProgress(error);
					}
				}
				else
				{
					string response = "approved or denied";
					if (command.GetType() == typeof(ApproveRequestCommand))
						response = "approved";
					if (command.GetType() == typeof(DenyRequestCommand))
						response = "denied";
					_feedback.SendProgress($"Request {personRequest.Id.GetValueOrDefault()} was succesfully {response}! Execute command took {stopwatch.Elapsed}");
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


		private void loadDataForResourceCalculation(IList<IPersonRequest> personRequests, IEnumerable<IAbsenceRequestValidator> validatorList)
		{
			var shouldLoadDataForResourceCalculation = validatorList != null && validatorList.Any(v => v is StaffingThresholdValidator);

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
			var totalPeriodIncludingMidnightAllTimeZones = new DateTimePeriod(totalPeriod.StartDateTime.AddDays(-1), totalPeriod.EndDateTime);

			if (shouldLoadDataForResourceCalculation)
			{
				_feedback.SendProgress($"Started loading data for requests in period {totalPeriodIncludingMidnightAllTimeZones}");
				_loadSchedulingStateHolderForResourceCalculation.Execute(_scenarioRepository.Current(),
																		 totalPeriodIncludingMidnightAllTimeZones,
																		 persons, _schedulingResultStateHolder);
			}
			else
			{
				_loadSchedulesForRequestWithoutResourceCalculation.Execute(_scenarioRepository.Current(),
																		   totalPeriodIncludingMidnightAllTimeZones,
																		   persons, _schedulingResultStateHolder);
			}
		}

		private static void simulateApproveAbsence(IRequest request, IRequestApprovalService absenceRequestApprovalService)
		{
			absenceRequestApprovalService.Approve(request);
		}


		private IPersonAccountBalanceCalculator getPersonAccountBalanceCalculator(IPersonAbsenceAccount personAccount, IAbsenceRequest absenceRequest, DateOnlyPeriod dateOnlyPeriod)
		{
			IPersonAccountBalanceCalculator personAccountBalanceCalculator;

			if (personAccount == null || personAccount.AccountCollection().IsEmpty())
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
			if (personAlreadyAbsentDuringRequestPeriod(absenceRequest))
			{
				process = new DenyAbsenceRequest
				{
					DenyReason = UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonAlreadyAbsent", absenceRequest.Person.PermissionInformation.Culture()),
					DenyOption = PersonRequestDenyOption.AlreadyAbsence
				};
			}
			return process;
		}

		private bool personAlreadyAbsentDuringRequestPeriod(IAbsenceRequest absenceRequest)
		{
			return
				_alreadyAbsentSpecification.IsSatisfiedBy(new AbsenceRequstAndSchedules(absenceRequest,
					_schedulingResultStateHolder, _budgetGroupState));
		}
	}
}