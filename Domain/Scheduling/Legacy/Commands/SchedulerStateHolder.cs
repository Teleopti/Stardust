using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class SchedulerStateHolder : ISchedulerStateHolder, IClearReferredShiftTradeRequests
	{
		private readonly ISchedulingResultStateHolder _schedulingResultState;
		private readonly IList<IPerson> _allPermittedPersons;
		private readonly ICollection<DateOnly> _daysToResourceCalculate = new HashSet<DateOnly>();
		private IScenario _requestedScenario;
		private readonly IList<IPersonRequest> _workingPersonRequests = new List<IPersonRequest>();
		private ShiftTradeRequestStatusCheckerWithSchedule _shiftTradeRequestStatusChecker;
		private CommonNameDescriptionSetting _commonNameDescription = new CommonNameDescriptionSetting();
		private CommonNameDescriptionSettingScheduleExport _commonNameDescriptionScheduleExport = new CommonNameDescriptionSettingScheduleExport();
		private DefaultSegment _defaultSegment = new DefaultSegment();
		private ICommonStateHolder _commonStateHolder;
        private IDictionary<Guid, IPerson> _filteredAgents;
		private Lazy<IDictionary<Guid, IPerson>> _combinedFilteredAgents;
		private IDictionary<Guid, IPerson> _filteredPersonsOvertimeAvailability;
		private IDictionary<Guid, IPerson> _filteredPersonsHourlyAvailability;
		private bool _filterOnOvertimeAvailability;
		private bool _filterOnHourlyAvailability;

		public SchedulerStateHolder(IScenario loadScenario, IDateOnlyPeriodAsDateTimePeriod loadPeriod, IEnumerable<IPerson> allPermittedPersons, IDisableDeletedFilter disableDeleteFilter, ISchedulingResultStateHolder schedulingResultStateHolder, ITimeZoneGuard timeZoneGuard)
		{
			TimeZoneInfo = timeZoneGuard.CurrentTimeZone();
			_requestedScenario = loadScenario;
			_commonStateHolder = new CommonStateHolder(disableDeleteFilter);
			RequestedPeriod = loadPeriod;
			_schedulingResultState = schedulingResultStateHolder;
			_allPermittedPersons = new List<IPerson>(allPermittedPersons);
			ResetFilteredPersons();
			ResetFilteredPersonsOvertimeAvailability();
		}

		public SchedulerStateHolder(ISchedulingResultStateHolder schedulingResultStateHolder, ICommonStateHolder commonStateHolder, ITimeZoneGuard timeZoneGuard)
		{
			_schedulingResultState = schedulingResultStateHolder;
			TimeZoneInfo = timeZoneGuard.CurrentTimeZone();
			_commonStateHolder = commonStateHolder;
			_allPermittedPersons = new List<IPerson>();
			ResetFilteredPersonsOvertimeAvailability();
		}

		public void SetRequestedScenario(IScenario scenario)
		{
			_requestedScenario = scenario;
		}

		public IList<IPerson> AllPermittedPersons => _allPermittedPersons;

		public bool ConsiderShortBreaks { get; set; } = true;

		public ISchedulingResultStateHolder SchedulingResultState => _schedulingResultState;

		public TimeZoneInfo TimeZoneInfo { get; set; }

		public IList<IPersonRequest> PersonRequests => _workingPersonRequests;
		
		public IDictionary<Guid, IPerson> FilteredCombinedAgentsDictionary => _combinedFilteredAgents.Value;
		
		public IDictionary<Guid, IPerson> FilteredAgentsDictionary => _filteredAgents;

		private IDictionary<Guid, IPerson> combinedFilters()
		{
			var filter = _filteredAgents;

			if (!_filterOnOvertimeAvailability && !_filterOnHourlyAvailability) return filter;

			if (_filterOnOvertimeAvailability)
				filter = filter.Intersect(_filteredPersonsOvertimeAvailability).ToDictionary(t => t.Key, t => t.Value);

			if(_filterOnHourlyAvailability)
				filter = filter.Intersect(_filteredPersonsHourlyAvailability).ToDictionary(t => t.Key, t => t.Value);

			return filter;
		}

		public IScheduleDictionary Schedules => SchedulingResultState.Schedules;

		public ICommonStateHolder CommonStateHolder => _commonStateHolder;

		/// <summary>
		/// Gets the days to recalculate.
		/// </summary>
		/// <value>The days to recalculate.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-05-04
		/// </remarks>
		public IEnumerable<DateOnly> DaysToRecalculate
		{
			get
			{
				return _daysToResourceCalculate.OrderBy(d => d.Date).ToArray();
			}
		}

		public IDateOnlyPeriodAsDateTimePeriod RequestedPeriod { get; set; }


		public IScenario RequestedScenario => _requestedScenario;

		/// <summary>
		/// Clears the days to recalculate.
		/// </summary>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-05-04
		/// </remarks>
		public void ClearDaysToRecalculate()
		{
			_daysToResourceCalculate.Clear();
		}

		public bool ChangedRequests()
		{
			return PersonRequests.Any(personRequest => personRequest.Changed);
		}

		public void ClearReferredShiftTradeRequests()
		{
			_shiftTradeRequestStatusChecker?.ClearReferredShiftTradeRequests();
		}

		public void LoadSchedules(IFindSchedulesForPersons findSchedulesForPersons, IPersonProvider personsProvider, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDateTimePeriod period)
		{
			if (findSchedulesForPersons == null) throw new ArgumentNullException(nameof(findSchedulesForPersons));
			if (period == null) throw new ArgumentNullException(nameof(period));

			SchedulingResultState.Schedules =
				findSchedulesForPersons.FindSchedulesForPersons(period, RequestedScenario, personsProvider, scheduleDictionaryLoadOptions, AllPermittedPersons);
		}
		
		public void LoadSettings(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
		{
			_commonNameDescription = repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey(CommonNameDescriptionSetting.Key, new CommonNameDescriptionSetting());
			_commonNameDescriptionScheduleExport = repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey(CommonNameDescriptionSettingScheduleExport.Key, new CommonNameDescriptionSettingScheduleExport());
			_defaultSegment = repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey("DefaultSegment", new DefaultSegment());
		}

		public void LoadCommonState(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
		{
			CommonStateHolder.LoadCommonStateHolder(repositoryFactory, unitOfWork);
			_schedulingResultState.ShiftCategories = CommonStateHolder.ShiftCategories;
		}

		public void LoadCommonStateForResourceCalculationOnly(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
		{
			CommonStateHolder.LoadCommonStateHolderForResourceCalculationOnly(repositoryFactory, unitOfWork);
			_schedulingResultState.ShiftCategories = CommonStateHolder.ShiftCategories;
		}

		public void LoadPersonRequests(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory, IPersonRequestCheckAuthorization authorization, int numberOfDaysToShowNonPendingRequests)
		{
			if (_shiftTradeRequestStatusChecker == null)
				_shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerWithSchedule(SchedulingResultState.Schedules, authorization);

			IPersonRequestRepository personRequestRepository = null;
			if (repositoryFactory != null)
				personRequestRepository = repositoryFactory.CreatePersonRequestRepository(unitOfWork);
			var referredSpecification = new ShiftTradeRequestReferredSpecification(_shiftTradeRequestStatusChecker);
			var okByMeSpecification = new ShiftTradeRequestOkByMeSpecification(_shiftTradeRequestStatusChecker);
			var afterLoadedPeriodSpecification = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(SchedulingResultState.Schedules.Period.VisiblePeriod);
			_workingPersonRequests.Clear();

			var period = new DateTimePeriod(DateTime.UtcNow.Date.AddDays(-numberOfDaysToShowNonPendingRequests), DateTime.SpecifyKind(DateTime.MaxValue.Date, DateTimeKind.Utc));

			IList<IPersonRequest> personRequests = new List<IPersonRequest>();

			if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler) && _requestedScenario.DefaultScenario)
				if (personRequestRepository != null)
					personRequests = personRequestRepository.FindAllRequestModifiedWithinPeriodOrPending(AllPermittedPersons, period);
			
			var requests = personRequests.FilterBySpecification(new All<IPersonRequest>()
				                                                .And(afterLoadedPeriodSpecification).Or(
					                                                new All<IPersonRequest>().AndNot(referredSpecification)
					                                                                         .AndNot(okByMeSpecification)));

			
			foreach (var personRequest in requests)
			{
				personRequest.Changed = false;
				_workingPersonRequests.Add(personRequest);
			}
		}

		/// <summary>
		/// Return common agent name description
		/// </summary>
		/// <param name="person"></param>
		/// <returns></returns>
		public string CommonAgentName(IPerson person)
		{
			return _commonNameDescription.BuildCommonNameDescription(person);
		}

		/// <summary>
		/// Return common agent name description for schedule export
		/// </summary>
		/// <param name="person"></param>
		/// <returns></returns>
		public string CommonAgentNameScheduleExport(IPerson person)
		{
			return _commonNameDescriptionScheduleExport.BuildCommonNameDescription(person);
		}

		/// <summary>
		/// Marks the date to be recalculated.
		/// </summary>
		/// <param name="dateToRecalculate">The date.</param>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-05-04
		/// </remarks>
		public void MarkDateToBeRecalculated(DateOnly dateToRecalculate)
		{
			_daysToResourceCalculate.Add(dateToRecalculate);
		}

        public void ResetFilteredPersons()
        {
	        var allPermittedPersonIds = AllPermittedPersons.Select(x => x.Id.GetValueOrDefault()).ToArray();
	        _filteredAgents =
		        SchedulingResultState.PersonsInOrganization.Where(p => Array.IndexOf(allPermittedPersonIds, p.Id.Value) >= 0)
			        .OrderBy(CommonAgentName)
			        .ToDictionary(p => p.Id.Value);
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void ResetFilteredPersonsOvertimeAvailability()
		{
			_filteredPersonsOvertimeAvailability = new Dictionary<Guid, IPerson>();
			_filterOnOvertimeAvailability = false;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void ResetFilteredPersonsHourlyAvailability()
		{
			_filteredPersonsHourlyAvailability = new Dictionary<Guid, IPerson>();
			_filterOnHourlyAvailability = false;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void FilterPersons(IList<IPerson> selectedPersons)
		{
			_filteredAgents = selectedPersons.OrderBy(CommonAgentName).ToDictionary(p => p.Id.Value);
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void FilterPersonsOvertimeAvailability(IEnumerable<IPerson> selectedPersons)
		{
			_filteredPersonsOvertimeAvailability = selectedPersons.OrderBy(CommonAgentName).ToDictionary(p => p.Id.Value);
			_filterOnOvertimeAvailability = true;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void FilterPersonsHourlyAvailability(IList<IPerson> selectedPersons)
		{
			_filteredPersonsHourlyAvailability = selectedPersons.OrderBy(CommonAgentName).ToDictionary(p => p.Id.Value);
			_filterOnHourlyAvailability = true;
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public void FilterPersons(HashSet<Guid> selectedGuids)
		{
			var selectedPersons = new Dictionary<Guid, IPerson>();
			foreach (var person in AllPermittedPersons)
			{
				if (selectedGuids.Contains(person.Id.Value) && !selectedPersons.ContainsKey(person.Id.Value))
				{
					selectedPersons.Add(person.Id.Value, person);
				}
			}
			_filteredAgents = selectedPersons.Values.OrderBy(CommonAgentName).ToDictionary(p => p.Id.Value);
			_combinedFilteredAgents = new Lazy<IDictionary<Guid, IPerson>>(combinedFilters);
		}

		public IPersonRequest RequestUpdateFromBroker(IPersonRequestRepository personRequestRepository, Guid personRequestId, IScheduleStorage scheduleStorage)
		{
			IPersonRequest updatedRequest = null;
			if (PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
				updatedRequest = personRequestRepository.Find(personRequestId);

			if (updatedRequest != null)
			{
				if (!SchedulingResultState.PersonsInOrganization.Contains(updatedRequest.Person)) //Do not try to update persons that are not loaded in scheduler
					return null;

				updatePersonAccountFromBroker(scheduleStorage, updatedRequest);

				var shiftTradeRequestReferredSpecification = new ShiftTradeRequestReferredSpecification(_shiftTradeRequestStatusChecker);
				var shiftTradeRequestOkByMeSpecification = new ShiftTradeRequestOkByMeSpecification(_shiftTradeRequestStatusChecker);
				var shiftTradeRequestAfterLoadedPeriodSpecification =
					new ShiftTradeRequestIsAfterLoadedPeriodSpecification(Schedules.Period.VisiblePeriod);


				if (shiftTradeRequestAfterLoadedPeriodSpecification.IsSatisfiedBy(updatedRequest) ||
				    (!shiftTradeRequestOkByMeSpecification.IsSatisfiedBy(updatedRequest) &&
				    !shiftTradeRequestReferredSpecification.IsSatisfiedBy(updatedRequest))) 
				{
					updatedRequest.Changed = false;
					_workingPersonRequests.Add(updatedRequest);
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
				if (Schedules.Period.VisiblePeriod.Contains(period))
					return;

				var person = absenceRequest.Person;
				IPersonAccountCollection personAbsenceAccounts;
				if (!SchedulingResultState.AllPersonAccounts.TryGetValue(person, out personAbsenceAccounts))
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

						account.CalculateUsed(scheduleStorage, Schedules.Scenario);
						var range = (IValidateScheduleRange) Schedules[person];
						range.ValidateBusinessRules(NewBusinessRuleCollection.MinimumAndPersonAccount(SchedulingResultState, SchedulingResultState.AllPersonAccounts));
					}
				}
			}
		}

		public IPersonRequest RequestDeleteFromBroker(Guid personRequestId)
		{
			IPersonRequest currentRequest =
				_workingPersonRequests.FirstOrDefault(r => r.Id.GetValueOrDefault(Guid.Empty) == personRequestId);
			if (currentRequest != null)
			{
				_workingPersonRequests.Remove(currentRequest);
			}

			return currentRequest;
		}

		public IUndoRedoContainer UndoRedoContainer { get; set; }

		public CommonNameDescriptionSetting CommonNameDescription => _commonNameDescription;

		public CommonNameDescriptionSettingScheduleExport CommonNameDescriptionScheduleExport => _commonNameDescriptionScheduleExport;

		public int DefaultSegmentLength => _defaultSegment.SegmentLength;

		public bool AgentFilter()
		{
			return _filteredAgents.Count != _allPermittedPersons.Count;
		}

		public void SetCommonStateHolder(ICommonStateHolder commonStateHolder)
		{
			_commonStateHolder = commonStateHolder;
		}
	}
}
