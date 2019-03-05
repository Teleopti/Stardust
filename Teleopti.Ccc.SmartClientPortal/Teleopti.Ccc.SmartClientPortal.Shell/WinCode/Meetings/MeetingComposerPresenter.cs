using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public class MeetingComposerPresenter : IInitiatorIdentifier, IDisposable
    {
        private SchedulingScreenState _schedulingScreenState;
        private readonly IMeetingComposerView _view;
        private readonly IMeetingViewModel _model;
	    private readonly IDisableDeletedFilter _disableDeletedFilter;
	    private readonly IScheduleStorageFactory _scheduleStorageFactory;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly DateOnly _minDate = new DateOnly(DateHelper.MinSmallDateTime);
        private readonly DateOnly _maxDate = new DateOnly(DateHelper.MaxSmallDateTime);
        private Guid _instanceId = Guid.NewGuid();
        protected static CommonNameDescriptionSetting CommonNameDescription;
        private bool _canClose = true;
        private bool _isDirty;
        private bool _disposed;
    	private bool _trySave;

    	public MeetingComposerPresenter(IMeetingComposerView view, IMeetingViewModel model, IDisableDeletedFilter disableDeletedFilter, IScheduleStorageFactory scheduleStorageFactory)
        {
            _view = view;
            _model = model;
    		_disableDeletedFilter = disableDeletedFilter;
	        _scheduleStorageFactory = scheduleStorageFactory;
			

			RepositoryFactory = new RepositoryFactory();
            UnitOfWorkFactory = Infrastructure.UnitOfWork.UnitOfWorkFactory.Current;
        }

		public MeetingComposerPresenter(IMeetingComposerView view, IMeetingViewModel model, IDisableDeletedFilter disableDeletedFilter, SchedulingScreenState schedulingScreenState, IScheduleStorageFactory scheduleStorageFactory, ITimeZoneGuard timeZoneGuard)
			: this(view, model,disableDeletedFilter, scheduleStorageFactory)
        {
            _schedulingScreenState = schedulingScreenState;
			_timeZoneGuard = timeZoneGuard;

			if (_schedulingScreenState != null && model != null)
            {
                DateOnlyPeriod validPeriod = _schedulingScreenState.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
                _minDate = validPeriod.StartDate;
                _maxDate = validPeriod.EndDate;
            }
        }

        public static MeetingViewModel CreateDefaultMeeting(IPerson organizer, ISchedulerStateHolder schedulerStateHolder, DateOnly startDate, IEnumerable<IPerson> participants, INow now, ITimeZoneGuard timeZoneGuard)
        {
            return CreateDefaultMeeting(organizer, schedulerStateHolder.RequestedScenario,
										schedulerStateHolder.CommonStateHolder.Activities.NonDeleted().FirstOrDefault(), startDate, participants,
                                        schedulerStateHolder.CommonNameDescription, timeZoneGuard.CurrentTimeZone(), now);
        }

        public static MeetingViewModel CreateDefaultMeeting(IPerson organizer, IScenario scenario, IActivity activity, DateOnly startDate, IEnumerable<IPerson> participants, CommonNameDescriptionSetting commonNameDescriptionSetting, TimeZoneInfo timeZoneInfo, INow now)
        {
            var meeting = new Meeting(organizer,
                                           participants.Select(p => (IMeetingPerson)new MeetingPerson(p, false)),
                                           string.Empty, string.Empty, string.Empty, activity, scenario)
                                   {
                                       TimeZone = timeZoneInfo,
                                       StartDate = startDate
                                   };
            CreateMeetingDefaultTime(meeting,now.ServerDateTime_DontUse().TimeOfDay);
            return new MeetingViewModel(meeting,commonNameDescriptionSetting);
        }

        public static void CreateMeetingDefaultTime(IMeeting meeting, TimeSpan timeNow)
        {
            DateOnly startDate = meeting.StartDate;
            if (TimeHelper.FitToDefaultResolution(timeNow, 30).Days > timeNow.Days)
                startDate = meeting.StartDate.AddDays(1);
            meeting.StartDate = startDate;
            meeting.EndDate = startDate;
            meeting.StartTime = new TimeSpan(TimeHelper.FitToDefaultResolution(timeNow, 30).Hours, TimeHelper.FitToDefaultResolution(timeNow, 30).Minutes, 0);
            meeting.EndTime = meeting.StartTime.Add(TimeSpan.FromHours(1));
        }

        public DateOnly MinDate
        {
            get {
                return _minDate;
            }
        }

        public DateOnly MaxDate
        {
            get {
                return _maxDate;
            }
        }

        public void Initialize()
        {
            var persons = _model.Meeting.MeetingPersons.Select(meetingPerson => meetingPerson.Person).ToList();

	        if (_schedulingScreenState != null &&
	            persons.Any(person => !_schedulingScreenState.SchedulerStateHolder.ChoosenAgents.Contains(person)))
	        {
		        _schedulingScreenState = null;
	        }

            if (_schedulingScreenState == null)
            {
                _view.DisableWhileLoadingStateHolder();
                _view.StartLoadingStateHolder();
            }
            else
            {
                var availablePersons = _schedulingScreenState.SchedulerStateHolder.SchedulingResultState.LoadedAgents;
	            var multi = _schedulingScreenState.SchedulerStateHolder.CommonStateHolder.MultiplicatorDefinitionSets;

                _schedulingScreenState = new SchedulingScreenState(_disableDeletedFilter, new SchedulerStateHolder(_schedulingScreenState.SchedulerStateHolder.RequestedScenario,
                                                                 _schedulingScreenState.SchedulerStateHolder.RequestedPeriod,
																 availablePersons, new DisableDeletedFilter(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make())),new SchedulingResultStateHolder()));
                _schedulingScreenState.SchedulerStateHolder.SchedulingResultState.LoadedAgents = new List<IPerson>(availablePersons);
					((List<IMultiplicatorDefinitionSet>)_schedulingScreenState.SchedulerStateHolder.CommonStateHolder.MultiplicatorDefinitionSets).AddRange(multi);
            }

            _view.SetRecurrentMeetingActive(_model.IsRecurring);
            _model.PropertyChanged += _model_PropertyChanged;
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _isDirty = true;
        }

        public void OnSchedulerStateHolderRequested()
        {
            ReloadSchedulerStateHolder();
        }

        public void OnSchedulerStateHolderLoaded()
        {
            _view.EnableAfterLoadingStateHolder();
        }

        private void ReloadSchedulerStateHolder()
        {
            var requestedPeriod =
                new DateOnlyPeriod(_model.StartDate.AddDays(-1), _model.RecurringEndDate.AddDays(2));
			_schedulingScreenState = new SchedulingScreenState(_disableDeletedFilter, new SchedulerStateHolder(_model.Meeting.Scenario, new DateOnlyPeriodAsDateTimePeriod(requestedPeriod, Model.TimeZone),
															 new List<IPerson>(), _disableDeletedFilter, new SchedulingResultStateHolder()));

			var stateLoader = new SchedulerStateLoader(_schedulingScreenState,
				RepositoryFactory,
				UnitOfWorkFactory,
				new LazyLoadingManagerWrapper(),
				_scheduleStorageFactory, _timeZoneGuard);

            stateLoader.LoadOrganization();
        }

        public static CommonNameDescriptionSetting GetCommonNameDescriptionSetting(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
        {
            if (CommonNameDescription==null)
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    ISettingDataRepository settingDataRepository =
                        repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork);
                    CommonNameDescription = settingDataRepository.FindValueByKey("CommonNameDescription",
                                                                                        new CommonNameDescriptionSetting());
                }
            }
            return CommonNameDescription;
        }

        public CommonNameDescriptionSetting CommonNameDescriptionSetting
        {
            get { return GetCommonNameDescriptionSetting(UnitOfWorkFactory, RepositoryFactory); }
        }

        public bool IsMeetingValid()
        {
            string messageTobeDisplayed = string.Empty;

            if (string.IsNullOrEmpty(_model.GetSubject(new NoFormatting())))
            {
                messageTobeDisplayed = UserTexts.Resources.MeetingSubjectRequired;
            }

            if ((_model.Participants.Length == 0))
            {
                messageTobeDisplayed = UserTexts.Resources.MeetingParticipantsRequired;
            }

			if (_model.Activity == null)
			{
				messageTobeDisplayed = UserTexts.Resources.MeetingActivityRequired;
			}

            if (!string.IsNullOrEmpty(messageTobeDisplayed))
            {
                _view.ShowErrorMessage(
                    messageTobeDisplayed,
                    UserTexts.Resources.InvalidRequest);
                   
                return false;
            }

            return true;
        }

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; set; }
        protected IRepositoryFactory RepositoryFactory { get; set; }

        public IMeetingViewModel Model
        {
            get { return _model; }
        }

        public void DeleteMeeting()
        {
            IMeeting currentMeeting = _model.Meeting;
            if (currentMeeting != null)
            {
                using (IUnitOfWork unitOfWork = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    IMeetingRepository meetingRepository = RepositoryFactory.CreateMeetingRepository(unitOfWork);

                    if (currentMeeting.Id.HasValue)
                    {
                        currentMeeting = meetingRepository.Load(_model.Meeting.Id.GetValueOrDefault());
                        var persons = currentMeeting.MeetingPersons.Select(m => m.Person);
                        unitOfWork.Reassociate(persons);
                        if (!new MeetingParticipantPermittedChecker().ValidatePermittedPersons(persons, currentMeeting.StartDate, _view, PrincipalAuthorization.Current_DONTUSE()))
                            return;
                        currentMeeting.Snapshot();
                    }

                    meetingRepository.Remove(currentMeeting);
                    unitOfWork.PersistAll(this);
                }
				_isDirty = false;
            }

            _view.Close();
            _view.OnModificationOccurred(_model.Meeting, true);
        }

        public void SaveMeeting()
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var persons = _model.Meeting.MeetingPersons.Select(m => m.Person);
                //Reload So all data is there
                persons = RepositoryFactory.CreatePersonRepository(unitOfWork).FindPeople(persons);
                var checker = new MeetingParticipantPermittedChecker();
                if (!checker.ValidatePermittedPersons(persons, Model.StartDate, _view, PrincipalAuthorization.Current_DONTUSE()))
                    return;
            }
            if (!IsMeetingValid()) return;

            try
            {
                SaveOrUpdateMeeting();

                _view.OnModificationOccurred(_model.Meeting, false);
            }
            catch (OptimisticLockException)
            {
                _view.ShowErrorMessage(UserTexts.Resources.SomeoneChangedTheSameDataBeforeYouDot,
                                       UserTexts.Resources.OptimisticLockHeader);
            }
            CloseView();
        }

        private void SaveOrUpdateMeeting()
        {
            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
				IMeetingRepository meetingRepository = RepositoryFactory.CreateMeetingRepository(unitOfWork);
				if (_model.Meeting.Id.HasValue)
				{
					var meeting = meetingRepository.Load(_model.Meeting.Id.GetValueOrDefault());
					meeting.Snapshot();
                    unitOfWork.Merge(_model.Meeting);
                }
                else
                {
                    meetingRepository.Add(_model.Meeting);
                }
                unitOfWork.PersistAll(this);
            }
        }

        private void CloseView()
        {
            _isDirty = false;
            _canClose = true;
            _view.Close();
        }

        public Guid InitiatorId
        {
            get { return _instanceId; }
        }

        public SchedulingScreenState SchedulingScreenState
        {
            get { return _schedulingScreenState; }
        }

        public void ShowAddressBook()
        {
            var notDeletedPersons = SchedulingScreenState.SchedulerStateHolder.SchedulingResultState.LoadedAgents.Cast<IDeleteTag>().Where(person => !person.IsDeleted).Cast<IPerson>().ToList();
            var addressBookViewModel = new AddressBookViewModel(
                    ContactPersonViewModel.Parse(notDeletedPersons,
                                                 CommonNameDescriptionSetting).ToList(), _model.RequiredParticipants,
                    _model.OptionalParticipants);

            _view.ShowAddressBook(addressBookViewModel,_model.StartDate);
        }

        public void AddParticipantsFromAddressBook(AddressBookViewModel addressBookViewModel)
        {
            _model.AddParticipants(addressBookViewModel.RequiredParticipantList,
                                   addressBookViewModel.OptionalParticipantList);
            _view.OnParticipantsSet();
        }

        public void RecurrentMeetingUpdated()
        {
            _view.SetRecurrentMeetingActive(_model.IsRecurring);
        }

        public void OnClose()
        {
            if (_isDirty)
            {
                var result = _view.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade,
                                                           UserTexts.Resources.Meeting);
                if (result == DialogResult.Cancel)
                {
                    _canClose = false;
					_trySave = false;
                }
                else if (result == DialogResult.Yes)
                {
                    _canClose = false;
					_trySave = true;
                }
                else
                {
                    _canClose = true;
					_trySave = false;
                }
            }
            else
            {
                _canClose = true;
				_trySave = false;
            }
        }

		public bool TrySave()
		{
			return _trySave;
		}

        public bool CanClose()
        {
            return _canClose;
        }

        #region IDispose

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        /// <remarks>
        /// So far only managed code. No need implementing destructor.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual dispose method
        /// </summary>
        /// <param name="disposing">
        /// If set to <c>true</c>, explicitly called.
        /// If set to <c>false</c>, implicitly called from finalizer.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ReleaseManagedResources();
                }
                ReleaseUnmanagedResources();
                _disposed = true;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources()
        {
        }

        /// <summary>
        /// Releases the managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources()
        {
            if (_model!=null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
            }
        }

        #endregion

    	public void InvalidTimeInfo()
    	{
    		var messageTobeDisplayed = UserTexts.Resources.EndTimeMustBeGreaterOrEqualToStartTime;

			if (!string.IsNullOrEmpty(messageTobeDisplayed))
			{
				_view.ShowErrorMessage(
					messageTobeDisplayed,
					UserTexts.Resources.InvalidRequest);
			}
    	}

	    public void SetInstanceId(Guid newInstanceId)
	    {
		    _instanceId = newInstanceId;
	    }
    }
}