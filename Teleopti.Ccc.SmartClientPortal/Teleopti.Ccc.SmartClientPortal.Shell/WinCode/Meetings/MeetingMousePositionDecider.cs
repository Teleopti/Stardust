using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public enum MeetingMousePosition
    {
        None,
        OverStart,
        OverEnd,
        OverStartAndEnd
    }

    public interface IMeetingMousePositionDecider
    {
        MeetingMousePosition MeetingMousePosition { get; set; }
        TimeSpan DiffStart { get; set; }
        void CheckMousePosition(int mouseGridPositionX, Rectangle cellRect, LengthToTimeCalculator pixelConverter, DateTimePeriod period, int mouseCellPosition, TimeSpan startTime);
        RectangleF GetLayerRectangle(LengthToTimeCalculator pixelConverter, DateTimePeriod period, RectangleF clientRect);
    }

    public class MeetingMousePositionDecider : IMeetingMousePositionDecider
    {
        private readonly IMeetingSchedulesView _view;
        public MeetingMousePosition MeetingMousePosition { get; set; }
        public TimeSpan DiffStart { get; set; }

        public MeetingMousePositionDecider(IMeetingSchedulesView view)
        {
            _view = view;
            MeetingMousePosition = MeetingMousePosition.None;
        }

        public void CheckMousePosition(int mouseGridPositionX, Rectangle cellRect, LengthToTimeCalculator pixelConverter, DateTimePeriod period, int mouseCellPosition, TimeSpan startTime)
        {
            if(pixelConverter == null) throw new ArgumentNullException(nameof(pixelConverter));

            var meetingRect = GetLayerRectangle(pixelConverter, period, cellRect);

            if ((mouseGridPositionX < meetingRect.X + 4) && (mouseGridPositionX > meetingRect.X - 4))
            {
                _view.SetSizeWECursor();
                MeetingMousePosition = !_view.IsRightToLeft ? MeetingMousePosition.OverStart : MeetingMousePosition.OverEnd;
                return;
            }

            if ((mouseGridPositionX < meetingRect.Right + 4) && (mouseGridPositionX > meetingRect.Right - 4))
            {
                _view.SetSizeWECursor();
                MeetingMousePosition = !_view.IsRightToLeft ? MeetingMousePosition.OverEnd : MeetingMousePosition.OverStart;
                return;
            }

            if ((mouseGridPositionX > meetingRect.Left + 4) && (mouseGridPositionX < meetingRect.Right - 4))
            {
                _view.SetHandCursor();
                MeetingMousePosition = MeetingMousePosition.OverStartAndEnd;
                var dateTime = TimeZoneHelper.ConvertFromUtc(pixelConverter.DateTimeFromPosition(mouseCellPosition, _view.IsRightToLeft), TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
                var time = dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
                DiffStart = time.TimeOfDay.Subtract(startTime);

                return;
            }

            MeetingMousePosition = MeetingMousePosition.None;
            _view.SetDefaultCursor();  
        }

        public RectangleF GetLayerRectangle(LengthToTimeCalculator pixelConverter, DateTimePeriod period, RectangleF clientRect)
        {
            if (pixelConverter == null) throw new ArgumentNullException(nameof(pixelConverter));

            var x1 = pixelConverter.PositionFromDateTime(period.StartDateTime, _view.IsRightToLeft);
            var x2 = pixelConverter.PositionFromDateTime(period.EndDateTime, _view.IsRightToLeft);

            if (_view.IsRightToLeft)
            {
                var tmp = x1;
                x1 = x2;
                x2 = tmp;
            }

            if (x2 - x1 < 1)
                return RectangleF.Empty;
            return new RectangleF((float)(clientRect.Left + x1), clientRect.Top, (float)(x2 - x1), clientRect.Height);
        }
    }
}
