using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings
{
    public class MeetingComposerPresenter : IMessageBrokerModule, IDisposable
    {
        private ISchedulerStateHolder _schedulerStateHolder;
        private readonly IMeetingComposerView _view;
        private readonly IMeetingViewModel _model;
        private readonly DateOnly _minDate = new DateOnly(DateHelper.MinSmallDateTime);
        private readonly DateOnly _maxDate = new DateOnly(DateHelper.MaxSmallDateTime);
        private readonly Guid _moduleId = Guid.NewGuid();
        protected static CommonNameDescriptionSetting CommonNameDescription;
        private bool _canClose = true;
        private bool _isDirty;
        private bool _disposed;

        public MeetingComposerPresenter(IMeetingComposerView view, IMeetingViewModel model)
        {
            _view = view;
            _model = model;

            RepositoryFactory = new RepositoryFactory();
            UnitOfWorkFactory = Infrastructure.UnitOfWork.UnitOfWorkFactory.Current;
        }

        public MeetingComposerPresenter(IMeetingComposerView view, IMeetingViewModel model, ISchedulerStateHolder schedulerStateHolder) : this(view,model)
        {
            _schedulerStateHolder = schedulerStateHolder;

            if (_schedulerStateHolder != null && model != null)
            {
                DateOnlyPeriod validPeriod = _schedulerStateHolder.RequestedPeriod.ToDateOnlyPeriod(model.TimeZone);
                _minDate = validPeriod.StartDate;
                _maxDate = validPeriod.EndDate;
            }
        }

        public static MeetingViewModel CreateDefaultMeeting(IPerson organizer, ISchedulerStateHolder schedulerStateHolder,DateOnly startDate, IEnumerable<IPerson> participants )
        {
            return CreateDefaultMeeting(organizer, schedulerStateHolder.RequestedScenario,
										schedulerStateHolder.CommonStateHolder.ActiveActivities.FirstOrDefault(), startDate, participants,
                                        schedulerStateHolder.CommonNameDescription, schedulerStateHolder.TimeZoneInfo);
        }

        public static MeetingViewModel CreateDefaultMeeting(IPerson organizer, IScenario scenario, IActivity activity, DateOnly startDate, IEnumerable<IPerson> participants, CommonNameDescriptionSetting commonNameDescriptionSetting, ICccTimeZoneInfo timeZoneInfo)
        {
            var meeting = new Meeting(organizer,
                                           participants.Select(p => (IMeetingPerson)new MeetingPerson(p, false)),
                                           string.Empty, string.Empty, string.Empty, activity, scenario)
                                   {
                                       TimeZone = timeZoneInfo,
                                       StartDate = startDate
                                   };
            CreateMeetingDefaultTime(meeting,DateTime.Now.TimeOfDay);
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

            if (_schedulerStateHolder != null && persons.Any(person => !_schedulerStateHolder.AllPermittedPersons.Contains(person))) {_schedulerStateHolder = null;}

            if (_schedulerStateHolder == null)
            {
                _view.DisableWhileLoadingStateHolder();
                _view.StartLoadingStateHolder();
            }
            else
            {
                var availablePersons = _schedulerStateHolder.SchedulingResultState.PersonsInOrganization;
                _schedulerStateHolder = new SchedulerStateHolder(_schedulerStateHolder.RequestedScenario,
                                                                 _schedulerStateHolder.RequestedPeriod,
                                                                 availablePersons);
                _schedulerStateHolder.SchedulingResultState.PersonsInOrganization = new List<IPerson>(availablePersons);
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
                new DateOnlyPeriod(_model.StartDate.AddDays(-1), _model.RecurringEndDate.AddDays(2)).ToDateTimePeriod(
                    Model.TimeZone);
            _schedulerStateHolder = new SchedulerStateHolder(_model.Meeting.Scenario, requestedPeriod,
                                                             new List<IPerson>());
                                        
            var stateLoader = new SchedulerStateLoader(
                                                    _schedulerStateHolder,
                                                    RepositoryFactory,
                                                    UnitOfWorkFactory,
					                                new LazyLoadingManagerWrapper());
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

            if (string.IsNullOrEmpty(_model.Subject))
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

        //private bool ValidationOnPermittedList()
        //{
        //    foreach (IPerson person in _model.Meeting.MeetingPersons.Select(m => m.Person))
        //    {
        //        if (!TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyMeetings,Model.StartDate,person))
        //        {
        //            string message = string.Format(System.Globalization.CultureInfo.CurrentUICulture,
        //                                             UserTexts.Resources.MeetingErrorMessageWithOneParameter, person.Name);

        //            _view.ShowErrorMessage(message, UserTexts.Resources.Permissions);

        //            return false;
        //        }
        //    }

        //    return true;
        //}

        public void SaveMeeting()
        {
            using (var unitOfWork = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var persons = _model.Meeting.MeetingPersons.Select(m => m.Person);
                //Reload So all data is there
                persons = RepositoryFactory.CreatePersonRepository(unitOfWork).FindPeople(persons);
                //unitOfWork.Reassociate(persons);
                var checker = new MeetingParticipantPermittedChecker();
                if (!checker.ValidatePermittedPersons(persons, Model.StartDate, _view, TeleoptiPrincipal.Current.PrincipalAuthorization))
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

        public Guid ModuleId
        {
            get { return _moduleId; }
        }

        public ISchedulerStateHolder SchedulerStateHolder
        {
            get { return _schedulerStateHolder; }
        }

        public void ShowAddressBook()
        {
            var notDeletedPersons = SchedulerStateHolder.SchedulingResultState.PersonsInOrganization.Cast<IDeleteTag>().Where(person => !person.IsDeleted).Cast<IPerson>().ToList();
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
                }
                else if (result == DialogResult.Yes)
                {
                    _canClose = false;
                    SaveMeeting();
                }
                else
                {
                    _canClose = true;
                }
            }
            else
            {
                _canClose = true;
            }
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
    }
}