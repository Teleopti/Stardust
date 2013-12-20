﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
	/// <summary>
	/// Class for holding winclient state
	/// </summary>
	public class SchedulerStateHolder : ISchedulerStateHolder, IClearReferredShiftTradeRequests
	{
		private readonly ISchedulingResultStateHolder _schedulingResultState;
		private readonly IList<IPerson> _allPermittedPersons;
		private readonly ICollection<DateOnly> _daysToResourceCalculate = new HashSet<DateOnly>();
		private DateTimePeriod? _loadedPeriod;
		private IScenario _requestedScenario;
		private readonly IList<IPersonRequest> _workingPersonRequests = new List<IPersonRequest>();
		private ShiftTradeRequestStatusCheckerWithSchedule _shiftTradeRequestStatusChecker;
		private TimeZoneInfo _timeZoneInfo = TeleoptiPrincipal.Current.Regional.TimeZone;
		private CommonNameDescriptionSetting _commonNameDescription = new CommonNameDescriptionSetting();
		private CommonNameDescriptionSettingScheduleExport _commonNameDescriptionScheduleExport = new CommonNameDescriptionSettingScheduleExport();
		private DefaultSegment _defaultSegment = new DefaultSegment();
		private readonly CommonStateHolder _commonStateHolder = new CommonStateHolder();
		private IDictionary<Guid, IPerson> _filteredPersons;
		private bool _considerShortBreaks = true;
		private const int _NUMBER_OF_PERSONREQUEST_DAYS = -14;


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public SchedulerStateHolder(IScenario loadScenario, IDateOnlyPeriodAsDateTimePeriod loadPeriod, IEnumerable<IPerson> allPermittedPersons)
			: this(loadScenario, loadPeriod, allPermittedPersons, new SchedulingResultStateHolder())
		{ }

		public SchedulerStateHolder(IScenario loadScenario, IDateOnlyPeriodAsDateTimePeriod loadPeriod, IEnumerable<IPerson> allPermittedPersons, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_requestedScenario = loadScenario;
			RequestedPeriod = loadPeriod;
			_schedulingResultState = schedulingResultStateHolder;
			_allPermittedPersons = new List<IPerson>(allPermittedPersons);
			ResetFilteredPersons();
		}

		public SchedulerStateHolder(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_schedulingResultState = schedulingResultStateHolder;
			_allPermittedPersons = new List<IPerson>();
		}



		public void SetRequestedScenario(IScenario scenario)
		{
			_requestedScenario = scenario;
		}

		public IList<IPerson> AllPermittedPersons
		{
			get { return _allPermittedPersons; }
		}

		public bool ConsiderShortBreaks
		{
			get { return _considerShortBreaks; }
			set { _considerShortBreaks = value; }
		}

		public ISchedulingResultStateHolder SchedulingResultState
		{
			get { return _schedulingResultState; }
		}

		public TimeZoneInfo TimeZoneInfo
		{
			get { return _timeZoneInfo; }
			set { _timeZoneInfo = value; }
		}

		public IList<IPersonRequest> PersonRequests
		{
			get { return _workingPersonRequests; }
		}

		public IDictionary<Guid, IPerson> FilteredPersonDictionary
		{
			get { return _filteredPersons; }
		}

		public IScheduleDictionary Schedules
		{
			get { return SchedulingResultState.Schedules; }
		}

		public CommonStateHolder CommonStateHolder
		{
			get { return _commonStateHolder; }
		}

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
				return _daysToResourceCalculate.OrderBy(d => d.Date).ToList();
			}
		}

		/// <summary>
		/// Gets the loaded schedule data period.
		/// </summary>
		/// <value>The loaded period.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-06-23
		/// </remarks>
		public DateTimePeriod? LoadedPeriod
		{
			get { return _loadedPeriod; }
		}

		public IDateOnlyPeriodAsDateTimePeriod RequestedPeriod { get; set; }


		public IScenario RequestedScenario
		{
			get { return _requestedScenario; }
		}

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
			_shiftTradeRequestStatusChecker.ClearReferredShiftTradeRequests();
		}

		public void LoadSchedules(IScheduleRepository scheduleRepository, IPersonProvider personsProvider, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IScheduleDateTimePeriod period)
		{
			if (scheduleRepository == null) throw new ArgumentNullException("scheduleRepository");
			if (period == null) throw new ArgumentNullException("period");

			SchedulingResultState.Schedules =
				scheduleRepository.FindSchedulesForPersons(period, RequestedScenario, personsProvider, scheduleDictionaryLoadOptions, AllPermittedPersons);

			_loadedPeriod = period.LoadedPeriod();

		}

		/// <summary>
		/// Load settings
		/// </summary>
		public void LoadSettings(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
		{
			_commonNameDescription = repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
			_commonNameDescriptionScheduleExport = repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey("CommonNameDescriptionScheduleExport", new CommonNameDescriptionSettingScheduleExport());
			_defaultSegment = repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork).FindValueByKey("DefaultSegment", new DefaultSegment());
		}

		public void LoadCommonState(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory)
		{
			CommonStateHolder.LoadCommonStateHolder(repositoryFactory, unitOfWork);
			_schedulingResultState.ShiftCategories = CommonStateHolder.ShiftCategories;
		}

		/// <summary>
		/// Load requests
		/// </summary>
		/// <param name="unitOfWork">The unit of work.</param>
		/// <param name="repositoryFactory">The repository factory.</param>
		/// <param name="persons">The persons.</param>
		/// <param name="authorization">Authorization check for person requests.</param>
		public void LoadPersonRequests(IUnitOfWork unitOfWork, IRepositoryFactory repositoryFactory, IPersonRequestCheckAuthorization authorization)
		{
			if (_shiftTradeRequestStatusChecker == null)
				_shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerWithSchedule(SchedulingResultState.Schedules, authorization);
				_shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerWithSchedule(SchedulingResultState.Schedules, authorization);

			IPersonRequestRepository personRequestRepository = null;
			if (repositoryFactory != null)
				personRequestRepository = repositoryFactory.CreatePersonRequestRepository(unitOfWork);
			var referredSpecification = new ShiftTradeRequestReferredSpecification(_shiftTradeRequestStatusChecker);
			var okByMeSpecification = new ShiftTradeRequestOkByMeSpecification(_shiftTradeRequestStatusChecker);
			var afterLoadedPeriodSpecification = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(SchedulingResultState.Schedules.Period.VisiblePeriod);
			_workingPersonRequests.Clear();

			var period = new DateTimePeriod(DateTime.UtcNow.Date.AddDays(_NUMBER_OF_PERSONREQUEST_DAYS), DateTime.SpecifyKind(DateTime.MaxValue.Date, DateTimeKind.Utc));

			IList<IPersonRequest> personRequests = new List<IPersonRequest>();

			if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler) && _requestedScenario.DefaultScenario)
				if (personRequestRepository != null)
					personRequests = personRequestRepository.FindAllRequestModifiedWithinPeriodOrPending(AllPermittedPersons, period);

			var requests =
				personRequests.FilterBySpecification(new All<IPersonRequest>().And(afterLoadedPeriodSpecification)
				                                                              .Or(new All<IPersonRequest>().AndNot(referredSpecification)
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
			_filteredPersons = (from p in SchedulingResultState.PersonsInOrganization
								where AllPermittedPersons.Contains(p)
								orderby CommonAgentName(p)
								select p).ToDictionary(p => p.Id.Value);
		}

		public void FilterPersons(IList<ITeam> selectedTeams)
		{
			List<IPerson> list = new List<IPerson>(AllPermittedPersons).FindAll(
				new PersonBelongsToTeamSpecification(RequestedPeriod.DateOnlyPeriod, selectedTeams).IsSatisfiedBy);
			_filteredPersons =
				(from p in list orderby CommonAgentName(p) select p).ToDictionary(p => p.Id.Value);

		}
		public void FilterPersons(IList<IPerson> selectedPersons)
		{
			_filteredPersons =
				(from p in selectedPersons orderby CommonAgentName(p) select p).ToDictionary(p => p.Id.Value);

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void FilterPersons(HashSet<Guid> selectedGuids)
		{
			var selectedPersons = new List<IPerson>();
			foreach (var person in AllPermittedPersons)
			{
				if (selectedGuids.Contains(person.Id.Value) && !selectedPersons.Contains(person))
					selectedPersons.Add(person);
			}
			_filteredPersons = (from p in selectedPersons orderby CommonAgentName(p) select p).ToDictionary(p => p.Id.Value);
		}

		public IPersonRequest RequestUpdateFromBroker(IPersonRequestRepository personRequestRepository, Guid personRequestId)
		{
			IPersonRequest updatedRequest = null;
			if (PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
				updatedRequest = personRequestRepository.Find(personRequestId);

			if (updatedRequest != null)
			{
				if (!SchedulingResultState.PersonsInOrganization.Contains(updatedRequest.Person)) //Do not try to update persons that are not loaded in scheduler
					return null;

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

		public CommonNameDescriptionSetting CommonNameDescription
		{
			get { return _commonNameDescription; }
		}

		public CommonNameDescriptionSettingScheduleExport CommonNameDescriptionScheduleExport
		{
			get { return _commonNameDescriptionScheduleExport; }
		}

		public int DefaultSegmentLength
		{
			get { return _defaultSegment.SegmentLength; }
		}
	}
}
