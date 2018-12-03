using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
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
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    /// <summary>
    /// Represents the tets class for the MeetingComposerPresenter.
    /// </summary>
    /// <remarks>
    /// Created by: SavaniN
    /// Created date: 12/10/2008
    /// </remarks>
    [TestFixture]
    public class MeetingComposerPresenterTest : IDisposable
    {
        private MockRepository _mocks;
        private MeetingViewModel _model;
        private IMeetingComposerView _view;
        private IRepositoryFactory _repositoryFactory;
	    private IScheduleStorageFactory _scheduleStorageFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IPerson _person;
        private IPerson _requiredPerson;
        private IPerson _optionalPerson;
        private IActivity _activity;
        private TimeZoneInfo _timeZone;
        private IScenario _scenario;
        private IMeeting _meeting;
        private MeetingComposerPresenter _target;
        private ISchedulerStateHolder _schedulerStateHolder;
        private DateOnlyPeriod _requestedPeriod;
        private CommonNameDescriptionSetting _commonNameDescriptionSetting;
        private readonly IList<IContract> _contractList = new List<IContract>();
        private readonly ICollection<IContractSchedule> _contractScheduleColl = new List<IContractSchedule>();
        private IPersonRepository _personRep;
        private INow _now;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingComposerView>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
	        _scheduleStorageFactory = _mocks.StrictMock<IScheduleStorageFactory>();
						_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            _personRep = _mocks.StrictMock<IPersonRepository>();
            _person = PersonFactory.CreatePerson("organizer", "1");
            _requiredPerson = PersonFactory.CreatePerson("required", "2");
            _optionalPerson = PersonFactory.CreatePerson("optional", "3");
            _activity = ActivityFactory.CreateActivity("Meeting");
            _timeZone = (TimeZoneInfo.Local);
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _commonNameDescriptionSetting = new CommonNameDescriptionSetting();
			_now = new ThisIsNow(new DateTime(2009, 10, 14, 15, 0, 0, DateTimeKind.Utc));
            _model = MeetingComposerPresenter.CreateDefaultMeeting(_person, _scenario, _activity,
                                                                   new DateOnly(2009, 10, 14), new List<IPerson>(),
                                                                   _commonNameDescriptionSetting, _timeZone, _now);
            _meeting = _model.Meeting;
            _meeting.EndDate = new DateOnly(2009, 10, 16);
            _meeting.StartTime = TimeSpan.FromHours(19);
            _meeting.EndTime = TimeSpan.FromHours(21);
            _meeting.Subject = "my subject";
            _meeting.Location = "my location";
            _meeting.Description = "my description";
            _model.AddParticipants(new List<ContactPersonViewModel> { new ContactPersonViewModel(_requiredPerson) },
                                   new List<ContactPersonViewModel> { new ContactPersonViewModel(_optionalPerson) });

            _requestedPeriod = new DateOnlyPeriod(_meeting.StartDate, _meeting.EndDate.AddDays(2));

            Expect.Call(_schedulerStateHolder.RequestedPeriod).Return(new DateOnlyPeriodAsDateTimePeriod(_requestedPeriod,_timeZone));
            _mocks.Replay(_schedulerStateHolder);
            _target = new MeetingComposerPresenterForTest(_view, _model, new SchedulingScreenState(null, _schedulerStateHolder), _unitOfWorkFactory, _repositoryFactory, _scheduleStorageFactory);
            _mocks.Verify(_schedulerStateHolder);
            _mocks.BackToRecord(_schedulerStateHolder);
        }

        [Test]
        public void CanHandleMeetingsRoundMidnight()
        {
            DateOnly startDate = _meeting.StartDate;
            var timeNow = new TimeSpan(23, 50, 0);
            MeetingComposerPresenter.CreateMeetingDefaultTime(_meeting, timeNow);
            Assert.AreEqual(startDate.AddDays(1), _meeting.StartDate);
            Assert.AreEqual(new TimeSpan(0), _meeting.StartTime);
            Assert.AreEqual(new TimeSpan(1, 0, 0), _meeting.EndTime);
        }

        [Test]
        public void VerifyModuleIdGetsAValue()
        {
            Assert.AreNotEqual(Guid.Empty, _target.InitiatorId);
        }

        [Test]
        public void VerifyCanGetStaticCommonNameDescriptionSetting()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var settingDataRepository = _mocks.StrictMock<ISettingDataRepository>();

            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork)).Return(settingDataRepository);
            Expect.Call(settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting()))
                .IgnoreArguments().Return(_commonNameDescriptionSetting);
            unitOfWork.Dispose();

            _mocks.ReplayAll();
            CommonNameDescriptionSetting setting =
                MeetingComposerPresenter.GetCommonNameDescriptionSetting(_unitOfWorkFactory, _repositoryFactory);
            Assert.AreEqual(_commonNameDescriptionSetting, setting);

            //Verify that database only is hit once
            setting = MeetingComposerPresenter.GetCommonNameDescriptionSetting(_unitOfWorkFactory, _repositoryFactory);
            Assert.AreEqual(_commonNameDescriptionSetting, setting);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanShowAddressBook()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var settingDataRepository = _mocks.StrictMock<ISettingDataRepository>();
            var schedulingResultStateHolder = new SchedulingResultStateHolder(new List<IPerson>
                                                                                                          {
                                                                                                              _person,
                                                                                                              _requiredPerson,
                                                                                                              _optionalPerson
                                                                                                          }, null, null);

            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(unitOfWork)).Return(settingDataRepository);
            Expect.Call(settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting()))
                .IgnoreArguments().Return(_commonNameDescriptionSetting);
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(schedulingResultStateHolder);
            unitOfWork.Dispose();
            _view.ShowAddressBook(null, _model.StartDate);
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            _target.ShowAddressBook();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanAddParticipantsFromAddressBook()
        {
            _view.OnParticipantsSet();

            _mocks.ReplayAll();
            var addressBookViewModel = new AddressBookViewModel(new List<ContactPersonViewModel>(),
                                                                                 new List<ContactPersonViewModel>(),
                                                                                 new List<ContactPersonViewModel>());
            _target.AddParticipantsFromAddressBook(addressBookViewModel);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanInitializeWithStateHolder()
        {
	        var disableDeleteFilter = new DisableDeletedFilter(new CurrentUnitOfWork(new FakeCurrentUnitOfWorkFactory(null)));
	        ISchedulerStateHolder schedulerStateHolder = new SchedulerStateHolder(_scenario,new DateOnlyPeriodAsDateTimePeriod(_requestedPeriod,_timeZone),
                                                                                  new List<IPerson>
                                                                                      {
                                                                                          _person,
                                                                                          _requiredPerson,
                                                                                          _optionalPerson
                                                                                      }, disableDeleteFilter, new SchedulingResultStateHolder(), new TimeZoneGuard());
            _target = new MeetingComposerPresenter(_view, _model, disableDeleteFilter, new SchedulingScreenState(null, schedulerStateHolder), null);
            _view.SetRecurrentMeetingActive(true);

            _mocks.ReplayAll();
            Assert.AreEqual(_meeting.StartDate, _target.MinDate);
            Assert.AreEqual(_meeting.EndDate.AddDays(2), _target.MaxDate);
            Assert.AreEqual(_model, _target.Model);
            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanInitializeWithoutStateHolder()
        {
            _view.DisableWhileLoadingStateHolder();
            _view.SetRecurrentMeetingActive(true);
            _view.StartLoadingStateHolder();

            _mocks.ReplayAll();
            _target = new MeetingComposerPresenterForTest(_view, _model, null, _unitOfWorkFactory, _repositoryFactory, _scheduleStorageFactory);

            Assert.AreEqual(new DateOnly(DateHelper.MinSmallDateTime), _target.MinDate);
            Assert.AreEqual(new DateOnly(DateHelper.MaxSmallDateTime), _target.MaxDate);

            _target.Initialize();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanLoadStateHolder()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            var absenceRepository = _mocks.StrictMock<IAbsenceRepository>();
            var activityRepository = _mocks.StrictMock<IActivityRepository>();
            var dayOffRepository = _mocks.StrictMock<IDayOffTemplateRepository>();
            var shiftCategoryRepository = _mocks.StrictMock<IShiftCategoryRepository>();
            var personRepository = _mocks.StrictMock<IPersonRepository>();
            var contractRepMock = _mocks.StrictMock<IContractRepository>();
            var contractScheduleRepMock = _mocks.StrictMock<IContractScheduleRepository>();
            var businessUnitRepository = _mocks.StrictMock<IBusinessUnitRepository>();
			var multi = _mocks.DynamicMock<IMultiplicatorDefinitionSetRepository>();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.AtLeastOnce();
            Expect.Call(_repositoryFactory.CreateAbsenceRepository(unitOfWork)).Return(absenceRepository);
            Expect.Call(_repositoryFactory.CreateActivityRepository(unitOfWork)).Return(activityRepository);
            Expect.Call(_repositoryFactory.CreateDayOffRepository(unitOfWork)).Return(dayOffRepository);
            Expect.Call(_repositoryFactory.CreateShiftCategoryRepository(unitOfWork)).Return(shiftCategoryRepository);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
            Expect.Call(_repositoryFactory.CreateContractRepository(unitOfWork)).Return(contractRepMock);
            Expect.Call(_repositoryFactory.CreateContractScheduleRepository(unitOfWork)).Return(contractScheduleRepMock);
            Expect.Call(_repositoryFactory.CreatePartTimePercentageRepository(unitOfWork)).Return(_mocks.DynamicMock<IPartTimePercentageRepository>());
            Expect.Call(_repositoryFactory.CreateBusinessUnitRepository(unitOfWork)).Return(businessUnitRepository);
			Expect.Call(_repositoryFactory.CreateMultiplicatorDefinitionSetRepository(unitOfWork)).Return(multi);

			unitOfWork.Dispose();
            LastCall.Repeat.AtLeastOnce();

            Expect.Call(absenceRepository.LoadAll()).Return(new List<IAbsence>());
            Expect.Call(activityRepository.LoadAll()).Return(new List<IActivity>());
            Expect.Call(dayOffRepository.LoadAll()).Return(new List<IDayOffTemplate>());
            Expect.Call(shiftCategoryRepository.FindAll()).Return(new List<IShiftCategory>());
            Expect.Call(personRepository.FindAllAgents(new DateOnlyPeriod(), true)).IgnoreArguments().Return(
                new List<IPerson>());
            Expect.Call(contractRepMock.FindAllContractByDescription()).Return(_contractList);
            Expect.Call(contractScheduleRepMock.LoadAllAggregate()).Return(_contractScheduleColl);
            Expect.Call(businessUnitRepository.LoadAllBusinessUnitSortedByName()).Return(new List<IBusinessUnit>());
	        Expect.Call(multi.LoadAll()).Return(new List<IMultiplicatorDefinitionSet>());

            _view.EnableAfterLoadingStateHolder();

            _mocks.ReplayAll();
            _target.OnSchedulerStateHolderRequested();
            _target.OnSchedulerStateHolderLoaded();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanDeleteMeeting()
        {
            var id = Guid.NewGuid();
            _meeting.SetId(id);
            var person = PersonFactory.CreatePerson("test");
            _meeting.AddMeetingPerson(new MeetingPerson(person, false));

            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateMeetingRepository(unitOfWork)).Return(meetingRepository);
            Expect.Call(meetingRepository.Load(id)).Return(_meeting);
            Expect.Call(()=>unitOfWork.Reassociate(new List<IPerson>())).IgnoreArguments();
            meetingRepository.Remove(_meeting);
            Expect.Call(unitOfWork.PersistAll(_target)).Return(new List<IRootChangeInfo>());
            unitOfWork.Dispose();

            _view.Close();
            _view.OnModificationOccurred(_model.Meeting, true);

            _mocks.ReplayAll();
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.DeleteMeeting();
			}

			_mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSaveNewMeeting()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
            Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson>()).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateMeetingRepository(unitOfWork)).Return(meetingRepository);
            meetingRepository.Add(_meeting);
            Expect.Call(unitOfWork.PersistAll(_target)).Return(new List<IRootChangeInfo>());
            unitOfWork.Dispose();

            _view.Close();
            _view.OnModificationOccurred(_model.Meeting, false);

            _mocks.ReplayAll();
            _target.SaveMeeting();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSaveUpdatedMeeting()
        {
            _meeting.SetId(Guid.NewGuid());

            var meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
            Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson>()).IgnoreArguments();
            //Expect.Call(() => unitOfWork.Reassociate(new List<IPerson>())).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateMeetingRepository(unitOfWork)).Return(meetingRepository);
            Expect.Call(meetingRepository.Load(_meeting.Id.Value)).Return(_meeting);
            Expect.Call(unitOfWork.Merge(_model.Meeting)).Return(null);
            Expect.Call(unitOfWork.PersistAll(_target)).Return(new List<IRootChangeInfo>());
            unitOfWork.Dispose();

            _view.Close();
            _view.OnModificationOccurred(_model.Meeting, false);

            _mocks.ReplayAll();
            _target.SaveMeeting();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldHandleOptimisticLockWhenSavingUpdatedMeeting()
        {
            _meeting.SetId(Guid.NewGuid());

            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var meetingRepository = _mocks.StrictMock<IMeetingRepository>();

            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
            Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson>()).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateMeetingRepository(unitOfWork)).Return(meetingRepository);
            Expect.Call(meetingRepository.Load(_meeting.Id.Value)).Return(_meeting);
            Expect.Call(unitOfWork.Merge(_model.Meeting)).Return(null);
            Expect.Call(unitOfWork.PersistAll(_target)).Throw(new OptimisticLockException());
            unitOfWork.Dispose();

            _view.Close();
            _view.ShowErrorMessage("", "");
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            _target.SaveMeeting();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCannotSaveNewMeetingWithNonPermittedPersons()
        {
            var authorization = _mocks.StrictMock<IAuthorization>();
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
                Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson> { _person }).IgnoreArguments();
                Expect.Call(authorization.IsPermitted("", new DateOnly(), _person)).IgnoreArguments()
                   .Return(false).Repeat.AtLeastOnce();
                Expect.Call(unitOfWork.Dispose).IgnoreArguments();
                Expect.Call(() => _view.ShowErrorMessage("", "")).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					_target.SaveMeeting();
                }
            }
        }

        [Test]
        public void VerifyCanSaveNewMeetingWithNoLocation()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
            Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson>()).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreateMeetingRepository(unitOfWork)).Return(meetingRepository);
            meetingRepository.Add(_meeting);
            Expect.Call(unitOfWork.PersistAll(_target)).Return(new List<IRootChangeInfo>());
            unitOfWork.Dispose();

            _view.Close();
            _view.OnModificationOccurred(_model.Meeting, false);

            _mocks.ReplayAll();
            _model.Location = "";
            _target.SaveMeeting();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCannotSaveNewMeetingWithNoSubject()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
            Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson>()).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);

            _model.Subject = "";
            _view.ShowErrorMessage("", "");
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            _target.SaveMeeting();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCannotSaveNewMeetingWithNoValidActivity()
        {
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
            Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson>()).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);

            _model.Activity = null;
            _view.ShowErrorMessage("", "");
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            _target.SaveMeeting();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCannotSaveNewMeetingWithNoParticipants()
        {
            foreach (ContactPersonViewModel model in _model.OptionalParticipants)
            {
                _model.RemoveParticipant(model);
            }
            foreach (ContactPersonViewModel model in _model.RequiredParticipants)
            {
                _model.RemoveParticipant(model);
            }
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(_personRep);
            Expect.Call(_personRep.FindPeople(new List<IPerson>())).Return(new List<IPerson>()).IgnoreArguments();
            Expect.Call(unitOfWork.Dispose);
            _view.ShowErrorMessage("", "");
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            _target.SaveMeeting();
            _mocks.VerifyAll();
        }

		[Test]
		public void VerifyCannotSaveNewMeetingWithEndTimeEarlierThanStartTime()
		{
			_model.StartTime = new TimeSpan(11,0,0);
			_model.EndTime = new TimeSpan(10,0,0);
			_view.ShowErrorMessage("", "");
			LastCall.IgnoreArguments();

			_mocks.ReplayAll();
			_target.InvalidTimeInfo();
			_mocks.VerifyAll();
		}

        [Test]
        public void VerifyCanUpdateRecurringActive()
        {
            _view.SetRecurrentMeetingActive(true);
            _mocks.ReplayAll();
            _target.RecurrentMeetingUpdated();
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateDefaultMeetingWithSchedulerStateHolder()
        {
			var commonStateHolder = new CommonStateHolder(new DisableDeletedFilter(new CurrentUnitOfWork(new FakeCurrentUnitOfWorkFactory(null))));

            Expect.Call(_schedulerStateHolder.RequestedScenario).Return(_scenario);
            Expect.Call(_schedulerStateHolder.TimeZoneInfo).Return(_timeZone);
            Expect.Call(_schedulerStateHolder.CommonNameDescription).Return(_commonNameDescriptionSetting);
            Expect.Call(_schedulerStateHolder.CommonStateHolder).Return(commonStateHolder);

            _mocks.ReplayAll();
            MeetingViewModel meetingViewModel = MeetingComposerPresenter.CreateDefaultMeeting(_person,
                                                                                              _schedulerStateHolder,
                                                                                              _model.StartDate,
                                                                                              new List<IPerson>
                                                                                                  {
                                                                                                      _requiredPerson,
                                                                                                      _optionalPerson
                                                                                                  }, _now);
            Assert.AreEqual(2, meetingViewModel.RequiredParticipants.Count);
            _mocks.VerifyAll();
        }

		[Test]
		public void VerifyTrySave()
		{
			ISchedulerStateHolder schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_requestedPeriod, _timeZone),
																				  new List<IPerson>
                                                                                      {
                                                                                          _person,
                                                                                          _requiredPerson,
                                                                                          _optionalPerson
                                                                                      }, new DisableDeletedFilter(new CurrentUnitOfWork(new FakeCurrentUnitOfWorkFactory(null))), new SchedulingResultStateHolder(), new TimeZoneGuard());
			_target = new MeetingComposerPresenterForTest(_view, _model, new SchedulingScreenState(null, schedulerStateHolder), _unitOfWorkFactory,
														  _repositoryFactory, _scheduleStorageFactory);
			_target.TrySave();
			Assert.IsFalse(_target.TrySave());
		}
		

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyOnCloseBehavior()
        {
            ISchedulerStateHolder schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_requestedPeriod,_timeZone), 
                                                                                  new List<IPerson>
                                                                                      {
                                                                                          _person,
                                                                                          _requiredPerson,
                                                                                          _optionalPerson
                                                                                      }, new DisableDeletedFilter(new CurrentUnitOfWork(new FakeCurrentUnitOfWorkFactory(null))), new SchedulingResultStateHolder(), new TimeZoneGuard());
            _target = new MeetingComposerPresenterForTest(_view, _model, new SchedulingScreenState(null, schedulerStateHolder), _unitOfWorkFactory,
                                                          _repositoryFactory, _scheduleStorageFactory);

            _view.SetRecurrentMeetingActive(true);

            using (_mocks.Ordered())
            {
                Expect.Call(_view.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.Cancel);
                Expect.Call(_view.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.No);
                Expect.Call(_view.ShowConfirmationMessage("", "")).IgnoreArguments().Return(DialogResult.Yes);

            }
           
            _mocks.ReplayAll();

            _target.Initialize();

            Assert.IsTrue(_target.CanClose());
            _target.OnClose();
            Assert.IsTrue(_target.CanClose());

            _model.StartTime = TimeSpan.FromHours(17);
            _target.OnClose();
            Assert.IsFalse(_target.CanClose());

            _target.OnClose();
            Assert.IsTrue(_target.CanClose());

            _target.OnClose();
            Assert.IsFalse(_target.CanClose());

            _mocks.VerifyAll();
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_target != null)
                {
                    _target.Dispose();
                    _target = null;
                }
            }
        }

        #endregion
    }

    internal class MeetingComposerPresenterForTest : MeetingComposerPresenter
    {
        public MeetingComposerPresenterForTest(IMeetingComposerView view, MeetingViewModel model, SchedulingScreenState schedulingScreenState, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IScheduleStorageFactory scheduleStorageFactory)
            : base(view, model, new DisableDeletedFilter(new CurrentUnitOfWork(new FakeCurrentUnitOfWorkFactory(null))), schedulingScreenState, scheduleStorageFactory)
        {
            RepositoryFactory = repositoryFactory;
            UnitOfWorkFactory = unitOfWorkFactory;
            CommonNameDescription = null;
        }
    }
}
