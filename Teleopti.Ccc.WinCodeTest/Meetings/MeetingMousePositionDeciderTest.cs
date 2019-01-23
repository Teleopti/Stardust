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
    public class MeetingMousePositionDeciderTest
    {
        private MeetingMousePositionDecider _target;
        private IMeetingSchedulesView _view;
        private MockRepository _mocks;
        private DateOnly _startDate;
        private DateTimePeriod _period;
        private DateTime _start;
        private DateTime _end;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingSchedulesView>();
            _target = new MeetingMousePositionDecider(_view);
            _startDate = new DateOnly(2009, 10, 27);
            _start = new DateTime(2009, 10, 27, 8, 0, 0, DateTimeKind.Utc);
            _end = _start.AddHours(2);
            _period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(_start, _end, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

        }

        [Test]
        public void ShouldSetMeetingMousePositionStartAndSetCursor()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetSizeWECursor());
                Expect.Call(_view.IsRightToLeft).Return(false).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.CheckMousePosition(480, cellRect, pixelConverter, _period, 480, _start.TimeOfDay);
                Assert.AreEqual(MeetingMousePosition.OverStart, _target.MeetingMousePosition);
            }
        }

        [Test]
        public void ShouldSetMeetingMousePositionEndAndSetCursor()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetSizeWECursor());
                Expect.Call(_view.IsRightToLeft).Return(false).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.CheckMousePosition(600, cellRect, pixelConverter, _period, 600, _start.TimeOfDay);
                Assert.AreEqual(MeetingMousePosition.OverEnd, _target.MeetingMousePosition);
            }
        }

        [Test]
        public void ShouldSetMeetingMousePositionStartAndEndAndSetCursor()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetHandCursor());
                Expect.Call(_view.IsRightToLeft).Return(false).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.CheckMousePosition(520, cellRect, pixelConverter, _period, 520, _start.TimeOfDay);
                Assert.AreEqual(MeetingMousePosition.OverStartAndEnd, _target.MeetingMousePosition);
            }
        }

        [Test]
        public void ShouldSetMeetingMousePositionNoneAndSetCursor()
        {
            var start = _startDate.Date;
            var end = start.AddDays(1);
            var timePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(start, end, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
            var pixelConverter = new LengthToTimeCalculator(timePeriod, 1440);
            var cellRect = new Rectangle(0, 0, 1440, 0);

            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetDefaultCursor());
                Expect.Call(_view.IsRightToLeft).Return(false).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.CheckMousePosition(300, cellRect, pixelConverter, _period, 300, _start.TimeOfDay);
                Assert.AreEqual(MeetingMousePosition.None, _target.MeetingMousePosition);
            }
        }

        [Test]
        public void ShouldThrowExceptionOnNullPixelConverterGetLayerRectangle()
        {
	        Assert.Throws<ArgumentNullException>(
		        () => _target.CheckMousePosition(0, new Rectangle(), null, new DateTimePeriod(), 0, TimeSpan.Zero));
        }
    }
}
