using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingMoverTest
    {
        private MeetingMover _target;
        private MockRepository _mocks;
        private IMeetingSchedulesView _meetingSchedulesView;
        private IMeetingViewModel _meetingViewModel;
        private int _snapToMinutes;
        private DateOnly _startDate;

        [SetUp]
        public void Setup()
        {
            _snapToMinutes = 15;
            _mocks = new MockRepository();
            _meetingSchedulesView = _mocks.StrictMock<IMeetingSchedulesView>();
            _meetingViewModel = _mocks.StrictMock<IMeetingViewModel>();
            _target = new MeetingMover(_meetingSchedulesView, _meetingViewModel, _snapToMinutes, false);
            _startDate = new DateOnly(2009, 10, 27);
        }

        [Test]
        public void ShouldSetSnappedStartTime()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using(_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate);
                Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(3));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(0));
                Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromMinutes(15));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using(_mocks.Playback())
            {
                _target.Move(pixelConverter, 12, cellRect, MeetingMoveState.MovingStart, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldSetSnappedStartTimeToMaxEndTimeMinusSnapValue()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate).Repeat.AtLeastOnce();
                Expect.Call(_meetingViewModel.EndDate).Return(_startDate);
                Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(1)).Repeat.AtLeastOnce();
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(0));
                Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromMinutes(45));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 60, cellRect, MeetingMoveState.MovingStart, TimeSpan.Zero);
            }    
        }

        [Test]
        public void ShouldNotSetStartTimeToLesserThenTimePeriod()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate);
                Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(3));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(0));
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, -60, cellRect, MeetingMoveState.MovingStart, TimeSpan.Zero);
            }    
        }

        [Test]
        public void ShouldNotSetStartTimeGreaterThenMidnightMinusSnapValue()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1).AddHours(3);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1620);
            var cellRect = new Rectangle(0, 0, 1620, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate).Repeat.AtLeastOnce();
                Expect.Call(_meetingViewModel.EndDate).Return(_startDate.AddDays(1));
                Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(2));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(23));
                Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45)));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 1440, cellRect, MeetingMoveState.MovingStart, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldSetSnappedEndTime()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(1));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(0));
                Expect.Call(() => _meetingViewModel.EndTime = TimeSpan.FromMinutes(75));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 73, cellRect, MeetingMoveState.MovingEnd, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldSetSnappedEndTimeToMaxStartTimePlusSnapValue()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate);
                Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(1));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(0)).Repeat.AtLeastOnce();
                Expect.Call(() => _meetingViewModel.EndTime = TimeSpan.FromMinutes(15));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 5, cellRect,MeetingMoveState.MovingEnd, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldNotSetSnappedEndTimeGreaterThanTimePeriod()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate);
                Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(23));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(22));
                Expect.Call(() => _meetingViewModel.EndTime = TimeSpan.FromMinutes(0));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 1500, cellRect, MeetingMoveState.MovingEnd, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldSetSnappedStartAndEndTime()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate).Repeat.AtLeastOnce();
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(0));
                Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
                Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromMinutes(120));
                Expect.Call(() => _meetingViewModel.EndTime = TimeSpan.FromMinutes(180));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 120, cellRect,MeetingMoveState.MovingStartAndEnd, TimeSpan.Zero);
            }         
        }

        [Test]
        public void ShouldNotSetSnappedStartLesserThanTimePeriodWhenDraggingStartAndEndTime()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate).Repeat.AtLeastOnce();
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(1));
                Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
                Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromMinutes(0));
                Expect.Call(() => _meetingViewModel.EndTime = TimeSpan.FromHours(1));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, -60, cellRect,MeetingMoveState.MovingStartAndEnd, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldNotSetSnappedStartTimeGreaterThenMidnightMinusSnapValueWhenDraggingStartAndEnd()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1).AddHours(3);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1620);
            var cellRect = new Rectangle(0, 0, 1620, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate).Repeat.AtLeastOnce();
                Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(23));
                Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45)));
                Expect.Call(() =>_meetingViewModel.EndTime =TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(45)).Add(TimeSpan.FromHours(1)));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 1440, cellRect, MeetingMoveState.MovingStartAndEnd, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldNotSetEndTimeGreaterThenTimePeriodWhenDraggingStartAndEnd()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(_meetingViewModel.StartDate).Return(_startDate).Repeat.AtLeastOnce();
                Expect.Call(_meetingViewModel.MeetingDuration).Return(TimeSpan.FromHours(1));
                Expect.Call(_meetingViewModel.StartTime).Return(TimeSpan.FromHours(22));
                Expect.Call(() => _meetingViewModel.StartTime = TimeSpan.FromHours(23));
                Expect.Call(() => _meetingViewModel.EndTime = TimeSpan.FromHours(24));
                Expect.Call(() => _meetingSchedulesView.RefreshGridSchedules());
            }

            using (_mocks.Playback())
            {
                _target.Move(pixelConverter, 1425, cellRect, MeetingMoveState.MovingStartAndEnd, TimeSpan.Zero);
            }
        }

        [Test]
        public void ShouldThrowExceptionOnNullPixelConverterMoveStart()
        {
            Assert.Throws<ArgumentNullException>(() => _target.Move(null, 0, new Rectangle(),MeetingMoveState.MovingStart, TimeSpan.Zero));
        }

		[Test]
		public void ShouldThrowExceptionOnNullPixelConverterMoveEnd()
        {
			Assert.Throws<ArgumentNullException>(() => _target.Move(null, 0, new Rectangle(), MeetingMoveState.MovingEnd, TimeSpan.Zero));
        }

		[Test]
		public void ShouldThrowExceptionOnNullPixelConverterMoveStartAndEnd()
        {
			Assert.Throws<ArgumentNullException>(() => _target.Move(null, 0, new Rectangle(), MeetingMoveState.MovingStartAndEnd, TimeSpan.Zero));
        }
    }
}
