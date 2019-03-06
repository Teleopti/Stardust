using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	/*
	 *  DONT MOVE THIS TO DOMAIN
	 *
	 * Code here is simply copy/pasted from old stateholders, a new God object for schedulingscreen.
	 * This way domain code will get less dependencies at least...
	 */
	public class SchedulingScreenState
	{
		private readonly IDisableDeletedFilter _disableDeletedFilter;

		public SchedulingScreenState(IDisableDeletedFilter disableDeletedFilter, ISchedulerStateHolder schedulerStateHolder)
		{
			_disableDeletedFilter = disableDeletedFilter;
			SchedulerStateHolder = schedulerStateHolder;
		}

		//add more stuff from "domain stateholder" here
		public void Fill(IUnitOfWork uow)
		{
			using (_disableDeletedFilter.Disable())
			{
				var scheduleTags = ScheduleTagRepository.DONT_USE_CTOR(uow).LoadAll().OrderBy(t => t.Description).ToList();
				scheduleTags.Insert(0, NullScheduleTag.Instance);
				ScheduleTags = scheduleTags;

				var globalSettingDataRepository = GlobalSettingDataRepository.DONT_USE_CTOR(uow);
				CommonNameDescriptionScheduleExport = globalSettingDataRepository.FindValueByKey(CommonNameDescriptionSettingScheduleExport.Key, new CommonNameDescriptionSettingScheduleExport());
				DefaultSegmentLength = globalSettingDataRepository.FindValueByKey("DefaultSegment", new DefaultSegment()).SegmentLength;
				WorkflowControlSets = WorkflowControlSetRepository.DONT_USE_CTOR(uow).LoadAll();
			}

			ModifiedWorkflowControlSets = new List<IWorkflowControlSet>();
		}
		
		public ISchedulerStateHolder SchedulerStateHolder { get; }
		public IEnumerable<IScheduleTag> ScheduleTags { get; private set; }
		public CommonNameDescriptionSettingScheduleExport CommonNameDescriptionScheduleExport { get; private set; }
		public IEnumerable<IWorkflowControlSet> WorkflowControlSets { get; private set; }
		public ICollection<IWorkflowControlSet> ModifiedWorkflowControlSets { get; private set; }
		public int DefaultSegmentLength { get; private set; } = new DefaultSegment().SegmentLength;
		
		
		public bool AgentFilter()
		{
			return SchedulerStateHolder.FilteredAgentsDictionary.Count != SchedulerStateHolder.ChoosenAgents.Count;
		}
		
		public bool ChangedRequests()
		{
			return PersonRequests.Any(personRequest => personRequest.Changed);
		}
		
		
		public IPersonRequest RequestUpdateFromBroker(IPersonRequestRepository personRequestRepository, Guid personRequestId, IScheduleStorage scheduleStorage)
		{
			IPersonRequest updatedRequest = null;
			if (PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
				updatedRequest = personRequestRepository.Find(personRequestId);
			
			if (updatedRequest != null)
			{
				var notOvertimeRequestSpecification = new NotOvertimeRequestSpecification();
				if (!notOvertimeRequestSpecification.IsSatisfiedBy(updatedRequest))
				{
					return null;
				}

				if (!SchedulerStateHolder.SchedulingResultState.LoadedAgents.Contains(updatedRequest.Person)) //Do not try to update persons that are not loaded in scheduler
					return null;

				updatePersonAccountFromBroker(scheduleStorage, updatedRequest);

				var shiftTradeRequestReferredSpecification = new ShiftTradeRequestReferredSpecification(SchedulerStateHolder.ShiftTradeRequestStatusChecker);
				var shiftTradeRequestOkByMeSpecification = new ShiftTradeRequestOkByMeSpecification(SchedulerStateHolder.ShiftTradeRequestStatusChecker);
				var shiftTradeRequestAfterLoadedPeriodSpecification = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(SchedulerStateHolder.Schedules.Period.VisiblePeriod);

				if (shiftTradeRequestAfterLoadedPeriodSpecification.IsSatisfiedBy(updatedRequest) ||
					!shiftTradeRequestOkByMeSpecification.IsSatisfiedBy(updatedRequest) &&
					!shiftTradeRequestReferredSpecification.IsSatisfiedBy(updatedRequest)) 
				{
					updatedRequest.Changed = false;
					PersonRequests.Add(updatedRequest);
				}
			}

			return updatedRequest;
		}
		
		
		private void updatePersonAccountFromBroker(IScheduleStorage scheduleStorage, IPersonRequest updatedRequest)
		{
			var absenceRequest = updatedRequest.Request as IAbsenceRequest;
			if (absenceRequest != null && (updatedRequest.IsApproved || updatedRequest.IsAutoAproved))
			{				
				var period = absenceRequest.Period;
				if (SchedulerStateHolder.Schedules.Period.VisiblePeriod.Contains(period))
					return;

				var person = absenceRequest.Person;
				IPersonAccountCollection personAbsenceAccounts;
				if (!SchedulerStateHolder.SchedulingResultState.AllPersonAccounts.TryGetValue(person, out personAbsenceAccounts))
					return;

				var absence = absenceRequest.Absence;
				foreach (IPersonAbsenceAccount personAbsenceAccount in personAbsenceAccounts)
				{
					if (personAbsenceAccount.Absence != absence)
						continue;

					foreach (var account in personAbsenceAccount.AccountCollection())
					{
						if (account.StartDate > person.TerminalDate)
							continue;

						account.CalculateUsed(scheduleStorage, SchedulerStateHolder.Schedules.Scenario);
						var range = (IValidateScheduleRange) SchedulerStateHolder.Schedules[person];
						range.ValidateBusinessRules(NewBusinessRuleCollection.MinimumAndPersonAccount(SchedulerStateHolder.SchedulingResultState, SchedulerStateHolder.SchedulingResultState.AllPersonAccounts));
					}
				}
			}
		}
		
		public void LoadPersonRequests(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory, IPersonRequestCheckAuthorization authorization, int numberOfDaysToShowNonPendingRequests)
		{
			if (SchedulerStateHolder.ShiftTradeRequestStatusChecker == null)
				SchedulerStateHolder.ShiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerWithSchedule(SchedulerStateHolder.SchedulingResultState.Schedules, authorization);

			IPersonRequestRepository personRequestRepository = null;
			if (repositoryFactory != null)
				personRequestRepository = repositoryFactory.CreatePersonRequestRepository(unitOfWork);
			var referredSpecification = new ShiftTradeRequestReferredSpecification(SchedulerStateHolder.ShiftTradeRequestStatusChecker);
			var okByMeSpecification = new ShiftTradeRequestOkByMeSpecification(SchedulerStateHolder.ShiftTradeRequestStatusChecker);
			var notOvertimeRequestSpecification = new NotOvertimeRequestSpecification();
			var afterLoadedPeriodSpecification = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(SchedulerStateHolder.SchedulingResultState.Schedules.Period.VisiblePeriod);
			PersonRequests.Clear();

			var period = new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-numberOfDaysToShowNonPendingRequests), DateTime.SpecifyKind(DateTime.MaxValue.Date, DateTimeKind.Utc));

			IList<IPersonRequest> personRequests = new List<IPersonRequest>();

			if (PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler) && SchedulerStateHolder.RequestedScenario.DefaultScenario)
				if (personRequestRepository != null)
					personRequests = personRequestRepository.FindAllRequestModifiedWithinPeriodOrPending(SchedulerStateHolder.ChoosenAgents, period);
			
			var requests = personRequests.FilterBySpecification(new All<IPersonRequest>()
																.And(afterLoadedPeriodSpecification)
																.Or(new All<IPersonRequest>().AndNot(referredSpecification).AndNot(okByMeSpecification))
																.And(notOvertimeRequestSpecification));

			
			foreach (var personRequest in requests)
			{
				personRequest.Changed = false;
				PersonRequests.Add(personRequest);
			}
		}
		
		public IPersonRequest RequestDeleteFromBroker(Guid personRequestId)
		{
			IPersonRequest currentRequest =
				PersonRequests.FirstOrDefault(r => r.Id.GetValueOrDefault(Guid.Empty) == personRequestId);
			if (currentRequest != null)
			{
				PersonRequests.Remove(currentRequest);
			}

			return currentRequest;
		}
		
		public IList<IPersonRequest> PersonRequests { get; } = new List<IPersonRequest>();
	}
}