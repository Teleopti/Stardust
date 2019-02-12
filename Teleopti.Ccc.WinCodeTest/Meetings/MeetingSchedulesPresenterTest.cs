using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    public class MeetingSchedulesPresenterTest
    {
        private MockRepository _mocks;
        private IPerson _person;
        private MeetingSchedulesPresenter _target;
        private ISchedulerStateHolder _schedulerStateHolder;
        private DateOnly _startDate;
        private MeetingViewModel _model;
        private IMeetingSchedulesView _view;
        private IScenario _scenario;
        private DateOnlyPeriod _period;
        private ISchedulerStateLoader _schedulerStateLoader;
        private IMeetingSlotFinderService _meetingSlotFinderService;
        private IMeetingMover _meetingMover;
        private IMeetingMousePositionDecider _meetingMousePositionDecider;
	    private TimeZoneInfo _timeZone;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.DynamicMock<IMeetingSchedulesView>();
            _person = PersonFactory.CreatePerson();
			_timeZone = TimeZoneInfo.Utc;
			TimeZoneGuardForDesktop.Instance_DONTUSE.Set(_timeZone);
			_person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            _startDate = new DateOnly(2009, 10, 27);
            _period = new DateOnlyPeriod(_startDate, _startDate.AddDays(3));
            _scenario = _mocks.StrictMock<IScenario>();
            _schedulerStateLoader = _mocks.DynamicMock<ISchedulerStateLoader>();
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, _timeZone), new List<IPerson> { _person }, _mocks.DynamicMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder());
            _meetingSlotFinderService = _mocks.StrictMock<IMeetingSlotFinderService>();

		    _model = MeetingComposerPresenter.CreateDefaultMeeting(
			    _person,
			    _schedulerStateHolder,
			    _startDate,
			    new List<IPerson>(),
			    new ThisIsNow(_startDate.Date.AddHours(15))
			    );
            _model.StartTime = TimeSpan.FromHours(8);
            _model.EndTime = TimeSpan.FromHours(10);

            _meetingMover = _mocks.StrictMock<IMeetingMover>();
            _meetingMousePositionDecider = _mocks.StrictMock<IMeetingMousePositionDecider>();

            _target = new MeetingSchedulesPresenter(_view, _model, _schedulerStateHolder, _schedulerStateLoader, _meetingSlotFinderService, _meetingMover, _meetingMousePositionDecider);
        }

        [TearDown]
        public void Teardown()
        {
            _target.Dispose();
        }

        [Test]
        public void VerifyCanGetProperties()
        {
            Assert.AreEqual(0, _target.RowCount);
            Assert.IsFalse(_target.IsInitialized);
            Assert.AreEqual(_model, _target.Model);
        }

        [Test]
        public void VerifyInitialize()
        {
            _schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();

            _view.SetStartDate(_model.StartDate);
            _view.SetEndDate(_model.EndDate);
            _view.SetStartTime(_model.StartTime);
            _view.SetEndTime(_model.EndTime);
            _view.SetRecurringDates(_model.Meeting.GetRecurringDates());
            _view.SetCurrentDate(_model.StartDate);
			
            Expect.Call(() => _schedulerStateLoader.LoadSchedules(null)).IgnoreArguments();
            Expect.Call(() => _view.RefreshGrid());
            
            _mocks.ReplayAll();
            _target.Initialize();
            Assert.AreEqual(_model.StartDate,_target.CurrentDate);
            Assert.IsTrue(_target.IsInitialized);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyOnParticipantsSet()
        {
            _view.RefreshGrid();

            _mocks.ReplayAll();
            Assert.AreEqual(0,_target.RowCount);
            _target.OnParticipantsSet();
            Assert.IsNotNull(_target.ParticipantList);
            Assert.AreEqual(0,_target.ParticipantList.Count);
            Assert.AreEqual(0, _target.RowCount);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetStateHolder()
        {
            Assert.IsNotNull(_target.StateHolder);
        }

        [Test]
        public void VerifyCanSetEndTime()
        {
            _mocks.ReplayAll();
            _view.SetEndTime(_model.EndTime);
            _mocks.VerifyAll();
        }


        [Test]
        public void VerifyCanGetSuggestionsRowCount()
        {
            _view.RefreshGrid();
            _mocks.ReplayAll();
            Assert.AreEqual(0, _target.RowCount);
            _target.OnParticipantsSet();
            Assert.IsNotNull(_target.ParticipantList);
            Assert.AreEqual(0, _target.ParticipantList.Count);
            Assert.AreEqual(0, _target.RowCount);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallFinderForSlots()
        {
            _schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();

            Expect.Call(() => _schedulerStateLoader.LoadSchedules(null)).IgnoreArguments();
            Expect.Call(() => _view.RefreshGrid());
            Expect.Call(_meetingSlotFinderService.FindSlots(new DateOnly(2009, 11, 1), TimeSpan.FromHours(2),
                                                            TimeSpan.FromHours(12), TimeSpan.FromHours(14), null,
                                                            new List<IPerson>())).Return(new List<TimePeriod>());
            Expect.Call(_view.SetSuggestListStartTime).Return(new TimeSpan(12, 0, 0));
            Expect.Call(_view.SetSuggestListEndTime).Return(new TimeSpan(14, 0, 0));
       
            _mocks.ReplayAll();
            _target.SetCurrentDate(_startDate.AddDays(5));
            var count = _target.SuggestionsRowCount;
            Assert.That(count,Is.Not.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetAvailableDays()
        {
            _schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();

            Expect.Call(() => _schedulerStateLoader.LoadSchedules(null)).IgnoreArguments();
            Expect.Call(() => _view.RefreshGrid());
            Expect.Call(_meetingSlotFinderService.FindAvailableDays(new List<DateOnly>{new DateOnly(2009, 11, 1)}, TimeSpan.FromHours(2),
                                                            TimeSpan.FromHours(12), TimeSpan.FromHours(14), null,
                                                            new List<IPerson>())).IgnoreArguments().Return(new List<DateOnly>());
            Expect.Call(_view.SetSuggestListStartTime).Return(new TimeSpan(12, 0, 0));
            Expect.Call(_view.SetSuggestListEndTime).Return(new TimeSpan(14, 0, 0));

            _mocks.ReplayAll();
            _target.SetCurrentDate(_startDate.AddDays(5));
            var count = _target.GetAvailableDays;
            Assert.That(count, Is.Not.Null);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartDateFromCurrentDateForRecurringMeeting()
        {
            _model.RecurringEndDate = _startDate.AddDays(10);
            _view.SetEndDate(_model.EndDate.AddDays(1));
            _view.SetStartDate(_model.StartDate.AddDays(1));

            _mocks.ReplayAll();
            _target.SetStartDateFromCurrentDate(_model.StartDate.AddDays(1));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetStartDateFromCurrentDateForNonrecurringMeeting()
        {
            _view.SetEndDate(_model.EndDate.AddDays(1));
            _view.SetStartDate(_model.StartDate.AddDays(1));

            _mocks.ReplayAll();
            _target.SetStartDateFromCurrentDate(_model.StartDate.AddDays(1));
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanSetTimesFromEditor()
        {
            _view.SetEndTime(TimeSpan.FromHours(15));
            _view.SetStartTime(TimeSpan.FromHours(14));

            _mocks.ReplayAll();
            _target.SetTimesFromEditor(TimeSpan.FromHours(14),TimeSpan.FromHours(15));
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldGetStartTime()
		{
			_model.StartTime = TimeSpan.FromHours(8);
			
			Assert.AreEqual(TimeSpan.FromHours(8), _target.GetStartTime);
		}

		[Test]
		public void ShouldGetEndTime()
		{
			_model.EndTime = TimeSpan.FromHours(17);

			Assert.AreEqual(TimeSpan.FromHours(17), _target.GetEndTime);
		}

		[Test]
		public void ShouldPositionMeetingOnSelectedStartTimeLeaveInputOk()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _view.RefreshGrid());
			}

			using(_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeLeave("07:00");
			}
		}

		[Test]
		public void ShouldNotPositionMeetingOnSelectedStartTimeLeaveInputNotOk()
		{
			using(_mocks.Record())
			{
				Expect.Call(() => _view.SetStartTime(_model.StartTime));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeLeave("agajga");
			}
		}

		[Test]
		public void ShouldPositionMeetingOnSelectedEndTimeLeaveOnInputOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.RefreshGrid());
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeLeave("17:00");
			}
		}

		[Test]
		public void ShouldNotPositionMeetingOnSelectedEndTimeLeaveInputNotOk()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.SetEndTime(_model.EndTime));
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeLeave("agajga");
			}
		}

		[Test]
		public void ShouldPositionMeetingOnKeyDownEnterInSelectedStartTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.RefreshGrid());
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeKeyDown(Keys.Enter, "07:00");
			}
		}

		[Test]
		public void ShouldNotPositionMeetingOnKeyDownNotEnterInSelectedStartTime()
		{
			using (_mocks.Record())
			{
				//Not expecting anything
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerStartTimeKeyDown(Keys.A, "07:00");
			}
		}

		[Test]
		public void ShouldPositionMeetingOnKeyDownEnterInSelectedEndTime()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.RefreshGrid());
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeKeyDown(Keys.Enter, "17:00");
			}
		}

		[Test]
		public void ShouldNotPositionMeetingOnKeyDownNotEnterInSelectedEndTime()
		{
			using (_mocks.Record())
			{
				//Not expecting anything
			}

			using (_mocks.Playback())
			{
				_target.OnOutlookTimePickerEndTimeKeyDown(Keys.A, "17:00");
			}
		}


        [Test]
        public void GetPersonShouldReturnContainedPerson()
        {
            var personViewModel = new EntityContainer<IPerson>(_person);
            Assert.That(MeetingSchedulesPresenter.GetPerson(personViewModel),Is.EqualTo(_person));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnDefaultMergedPeriod()
        {
			var person = new Person();
            person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            _schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var expectedStart = new DateTime(2009, 10, 27, 0, 0, 0, DateTimeKind.Utc);
            var expectedEnd = expectedStart.AddDays(1);
            var expectedPeriod  = new DateTimePeriod(expectedStart, expectedEnd);

            using (_mocks.Record())
            {
                Expect.Call(scheduleDay.ProjectionService()).Return(projectionService);
                Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
                Expect.Call(visualLayerCollection.Period()).Return(null);
                Expect.Call(_schedulerStateHolder.Schedules).Return(scheduleDictionary);
                Expect.Call(scheduleDictionary[person]).Return(range);
                Expect.Call(range.ScheduledDay(_startDate)).Return(scheduleDay);
            }

            using (_mocks.Playback())
            {
                _target = new MeetingSchedulesPresenter(_view, _model, _schedulerStateHolder, _schedulerStateLoader, _meetingSlotFinderService, _meetingMover, _meetingMousePositionDecider);
                _target.Model.AddParticipants(new List<ContactPersonViewModel> { new ContactPersonViewModel(person) }, new List<ContactPersonViewModel>());
                _target.RecreateParticipantList();
                var period = _target.MergedOrDefaultPeriod();
                Assert.AreEqual(expectedPeriod, period);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnMergedPeriodWhenLayerExtendsToNextDay()
        {
            //Should return latest end time(it will be rounded up 1 hour if it contains minutes) + 1 hour
            var person = new Person();
            _schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            var range = _mocks.StrictMock<IScheduleRange>();
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var startFirst = new DateTime(2009, 10, 27, 11, 0, 0,DateTimeKind.Utc);
            var endFirst = new DateTime(2009, 10, 28, 2, 0, 0, DateTimeKind.Utc);
            var periodFirst = new DateTimePeriod(startFirst, endFirst);
            var expectedPeriod = new DateTimePeriod(startFirst, endFirst);

				using (_mocks.Record())
				{
					Expect.Call(scheduleDay.ProjectionService()).Return(projectionService);
					Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
					Expect.Call(visualLayerCollection.Period()).Return(periodFirst);
					Expect.Call(_schedulerStateHolder.Schedules).Return(scheduleDictionary);
					Expect.Call(scheduleDictionary[person]).Return(range);
					Expect.Call(range.ScheduledDay(_startDate)).Return(scheduleDay).IgnoreArguments();
				}

            using (_mocks.Playback())
            {
                _target = new MeetingSchedulesPresenter(_view, _model, _schedulerStateHolder, _schedulerStateLoader, _meetingSlotFinderService, _meetingMover, _meetingMousePositionDecider);
                _target.Model.AddParticipants(new List<ContactPersonViewModel> { new ContactPersonViewModel(person) }, new List<ContactPersonViewModel>());
                _target.RecreateParticipantList();
                var period = _target.MergedOrDefaultPeriod();
				Assert.That(period, Is.EqualTo(expectedPeriod));
            }
        }

        [Test]
        public void ShouldSetMeetingMoveStateNoneOnMouseDownAndMouseNotOverMeeting()
        {
            using(_mocks.Record())
            {
                Expect.Call(_meetingMousePositionDecider.MeetingMousePosition).Return(MeetingMousePosition.None).Repeat.AtLeastOnce();
            }

            using(_mocks.Playback())
            {
                _target.GridControlSchedulesMouseDown();
            }
        }

        [Test]
        public void ShouldSetMeetingMoveStateMovingEndOnMouseDownAndMouseOverEnd()
        {
            using (_mocks.Record())
            {
                Expect.Call(_meetingMousePositionDecider.MeetingMousePosition).Return(MeetingMousePosition.OverEnd).Repeat.AtLeastOnce();
                Expect.Call(() => _meetingMover.MeetingMoveState = MeetingMoveState.MovingEnd);
            }

            using (_mocks.Playback())
            {
                _target.GridControlSchedulesMouseDown();
            }
        }

        [Test]
        public void ShouldSetMeetingMoveStateMovingStartAndEndOnMouseDownAndMouseOverStartAndEnd()
        {
            using (_mocks.Record())
            {
                Expect.Call(_meetingMousePositionDecider.MeetingMousePosition).Return(MeetingMousePosition.OverStartAndEnd).Repeat.AtLeastOnce();
                Expect.Call(() => _meetingMover.MeetingMoveState = MeetingMoveState.MovingStartAndEnd);
            }

            using (_mocks.Playback())
            {
                _target.GridControlSchedulesMouseDown();
            }
        }

        [Test]
        public void ShouldSetMeetingMoveStateMovingStartOnMouseDownAndMouseOverStart()
        {
            using (_mocks.Record())
            {
                Expect.Call(_meetingMousePositionDecider.MeetingMousePosition).Return(MeetingMousePosition.OverStart).Repeat.AtLeastOnce();
                Expect.Call(() => _meetingMover.MeetingMoveState = MeetingMoveState.MovingStart);
            }

            using (_mocks.Playback())
            {
                _target.GridControlSchedulesMouseDown();
            }
        }

        [Test]
        public void ShouldRunOnMeetingTimeChangedOnMouseUpWhenMovingMeeting()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _view.OnMeetingTimeChanged());
                Expect.Call(() => _meetingMousePositionDecider.MeetingMousePosition = MeetingMousePosition.None);
                Expect.Call(_meetingMover.MeetingMoveState).Return(MeetingMoveState.MovingStart);
                Expect.Call(() => _meetingMover.MeetingMoveState = MeetingMoveState.None);
            }

            using (_mocks.Playback())
            {
                _target.GridControlSchedulesMouseUp();
            }
        }

        [Test]
        public void ShouldThrowExceptionOnNullPixelConverterMouseMove()
        {
            Assert.Throws<ArgumentNullException>(() => _target.GridControlSchedulesMouseMove(0, new Rectangle(), null, 0 ));
        }

        [Test]
        public void ShouldThrowExceptionOnNullPixelConverterGetLayerRectangle()
        {
            Assert.Throws<ArgumentNullException>(() => _target.GetLayerRectangle(null, new DateTimePeriod(), new RectangleF()));
        }
	}
}
