using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using log4net.Repository.Hierarchy;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Specification;
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

		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private readonly IBudgetGroupHeadCountSpecification _budgetGroupHeadCountSpecification;
		private readonly IBudgetGroupAllowanceSpecification _budgetGroupAllowanceSpecification;
		private readonly IAlreadyAbsentSpecification _alreadyAbsentSpecification;
		private readonly IResourceOptimization _resourceOptimizationHelper;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
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

		public MultiAbsenceRequestsUpdater(
			ICurrentScenario scenarioRepository,
			ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulesForRequestWithoutResourceCalculation,
			IRequestFactory requestFactory, 
			IAlreadyAbsentSpecification alreadyAbsentSpecification,
			IPersonRequestCheckAuthorization authorization,
			IBudgetGroupHeadCountSpecification budgetGroupHeadCountSpecification,
			IResourceOptimization resourceOptimizationHelper,
			IBudgetGroupAllowanceSpecification budgetGroupAllowanceSpecification,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
			ICommandDispatcher commandDispatcher,
			IStardustJobFeedback feedback, 
			ArrangeRequestsByProcessOrder arrangeRequestsByProcessOrder, 
			IScheduleDayChangeCallback scheduleDayChangeCallback, 
			ISchedulingResultStateHolder schedulingResultStateHolder, CascadingResourceCalculationContextFactory resourceCalculationContextFactory, IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, IPersonRepository personRepository, ISkillRepository skillRepository, IContractRepository contractRepository, IPartTimePercentageRepository partTimePercentageRepository, IContractScheduleRepository contractScheduleRepository, ISkillTypeRepository skillTypeRepository, IActivityRepository activityRepository, IAbsenceRepository absenceRepository, IPersonRequestRepository personRequestRepository, IDayOffTemplateRepository dayOffTemplateRepository)
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
			var aggregatedValidatorList = new HashSet<IAbsenceRequestValidator>();
			IList<IPersonRequest> personRequests;
			var stopwatch = new Stopwatch();

			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				stopwatch.Start();
				preloadData();
				personRequests = _personRequestRepository.Find(personRequestsIds);
				_personRepository.FindPeople(personRequests.Select(x => x.Person.Id.GetValueOrDefault()));
				stopwatch.Stop();
				_feedback.SendProgress($"Done preloading data! It took {stopwatch.Elapsed}");


				foreach (var personRequest in personRequests)
				{
					var person = personRequest.Person;
					if (person.WorkflowControlSet != null)
					{
						var mergedPeriod = person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((AbsenceRequest)personRequest.Request);
						aggregatedValidatorList.UnionWith(mergedPeriod.GetSelectedValidatorList());
					}
				}
			
				stopwatch.Start();
				loadDataForResourceCalculation(personRequests, aggregatedValidatorList);
				stopwatch.Stop();
				_feedback.SendProgress($"Done loading data for resource calculation! It took {stopwatch.Elapsed}");

				var seniority = _arrangeRequestsByProcessOrder.GetRequestsSortedBySeniority(personRequests);
				var firstComeFirstServe = _arrangeRequestsByProcessOrder.GetRequestsSortedByDate(personRequests);

					stopwatch.Start();
				using (_resourceCalculationContextFactory.Create(_schedulingResultStateHolder.Schedules, _schedulingResultStateHolder.Skills, true))
				{
					stopwatch.Stop();
					_feedback.SendProgress($"Done _resourceCalculationContextFactory.Create(..)! It took {stopwatch.Elapsed}");
					processOrderList(seniority);
					processOrderList(firstComeFirstServe);
				}
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
					string message = $"Optimistic lock when persisting request ({personRequest.Id.GetValueOrDefault()})! Number of retries: {count}";
					logger.Warn(message);
					_feedback.SendProgress(message);
				}
				_feedback.SendProgress($"Persisted request {personRequest.Id}.");
			}
		}

		private void preloadData()
		{
			_skillRepository.LoadAllSkills();
			_contractRepository.LoadAll();
			_skillTypeRepository.LoadAll();
			_partTimePercentageRepository.LoadAll();
			_contractScheduleRepository.LoadAllAggregate();
			_activityRepository.LoadAll();
			_absenceRepository.LoadAll();
			_dayOffTemplateRepository.LoadAll();
		}

		private void processOrderList(IList<IPersonRequest> requests)
		{
			var stopwatch = new Stopwatch();
			foreach (var personRequest in requests)
			{
				var absenceRequest = personRequest.Request as IAbsenceRequest;
				if(absenceRequest == null) continue;

				var agentTimeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
				var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

				var undoRedoContainer = new UndoRedoContainer(_scheduleDayChangeCallback, 400);

				var workflowControlSet = absenceRequest.Person.WorkflowControlSet;


				IPersonAccountCollection allAccounts;
				if (!_schedulingResultStateHolder.AllPersonAccounts.TryGetValue(absenceRequest.Person, out allAccounts))
					allAccounts = new PersonAccountCollection(absenceRequest.Person);

				var affectedPersonAbsenceAccount = allAccounts.Find(absenceRequest.Absence);
				var currentScenario = _scenarioRepository.Current();

				var mergedPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod(absenceRequest);
				var validatorList = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);
				var processAbsenceRequest = mergedPeriod.AbsenceRequestProcess;
				
				var personAccountBalanceCalculator = getPersonAccountBalanceCalculator(affectedPersonAbsenceAccount, absenceRequest, dateOnlyPeriod);
				
				setupUndoContainersAndTakeSnapshot(undoRedoContainer, allAccounts);
				
				processAbsenceRequest = checkIfPersonIsAlreadyAbsentDuringRequestPeriod(absenceRequest, processAbsenceRequest);

				var businessRules = NewBusinessRuleCollection.Minimum();

				var requestApprovalServiceScheduler = _requestFactory.GetRequestApprovalService(businessRules, currentScenario, _schedulingResultStateHolder);
				
				simulateApproveAbsence(absenceRequest, requestApprovalServiceScheduler);

				//Will issue a rollback for simulated schedule data
				processAbsenceRequest = handleInvalidSchedule(processAbsenceRequest, personRequest.Person);

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

				stopwatch.Restart();
				processAbsenceRequest.Process(absenceRequest,
											  requiredForProcessingAbsenceRequest,
											  requiredForHandlingAbsenceRequest,
											  validatorList);
				stopwatch.Stop();
				_feedback.SendProgress($"processAbsenceRequest.process(..) took {stopwatch.Elapsed}");
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
					var denyOption = PersonRequestDenyOption.None;
					if (personRequest.IsAlreadyAbsent)
						denyOption = PersonRequestDenyOption.AlreadyAbsence;
					else if (personRequest.IsExpired)
						denyOption = PersonRequestDenyOption.RequestExpired;
					command = new DenyRequestCommand()
					{
						PersonRequestId = personRequest.Id.GetValueOrDefault(),
						DenyReason = personRequest.DenyReason,
						DenyOption = denyOption
					};
				}
				_commandDispatcher.Execute(command);

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
					_feedback.SendProgress($"Request {personRequest.Id.GetValueOrDefault()} was succesfully {response}!");
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

		private IProcessAbsenceRequest denyAbsenceRequest(string reasonResourceKey, bool alreadyAbsence = false)
		{
			_denyAbsenceRequest.DenyReason = reasonResourceKey;
			_denyAbsenceRequest.DenyOption = alreadyAbsence ? PersonRequestDenyOption.AlreadyAbsence : PersonRequestDenyOption.None;
			return _denyAbsenceRequest;
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
			var totalPeriodIncludingMidnight = new DateTimePeriod(totalPeriod.StartDateTime.Date, totalPeriod.EndDateTime);

			if (shouldLoadDataForResourceCalculation)
			{
				_feedback.SendProgress($"Started loading data for requests in period {totalPeriodIncludingMidnight}");
				_loadSchedulingStateHolderForResourceCalculation.Execute(_scenarioRepository.Current(),
																		 totalPeriodIncludingMidnight,
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
			if (personAlreadyAbsentDuringRequestPeriod(absenceRequest))
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

		//may be remove this whole code
		private IProcessAbsenceRequest handleInvalidSchedule(IProcessAbsenceRequest process, IPerson person)
		{
			if (process.GetType() == typeof(DenyAbsenceRequest)) return process;
			if (satisfySchedule(person))
			{
				process = _pendingAbsenceRequest;
			}
			return process;
		}

		private bool satisfySchedule(IPerson person)
		{
			try
			{
				foreach (KeyValuePair<IPerson, IScheduleRange> scheduleRange in _schedulingResultStateHolder.Schedules.Where(x => x.Key == person))
				{
					var period =
						scheduleRange.Value.Period.ToDateOnlyPeriod(
							scheduleRange.Key.PermissionInformation.DefaultTimeZone());
					var schedules = scheduleRange.Value.ScheduledDayCollection(period);
					foreach (IScheduleDay scheduleDay in schedules)
					{
						var ass = scheduleDay.PersonAssignment();
						if (ass != null)
						{
							ass.CheckRestrictions();
						}
					}

				}
			}
			catch (ValidationException)
			{
				return true;
			}
			return false;
		}
	}
	
}