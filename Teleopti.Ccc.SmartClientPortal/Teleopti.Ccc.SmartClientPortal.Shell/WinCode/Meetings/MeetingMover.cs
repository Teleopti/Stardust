using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public enum MeetingMoveState
    {
        None,
        MovingStart,
        MovingEnd,
        MovingStartAndEnd
    }

    public interface IMeetingMover
    {
        void Move(ILengthToTimeCalculator pixelConverter, int mouseCellPosition, Rectangle cellRect, MeetingMoveState moveState, TimeSpan diffStart);
        MeetingMoveState MeetingMoveState { get; set; }
    }

    public class MeetingMover : IMeetingMover
    {
        private readonly IMeetingSchedulesView _schedulesView;
        private readonly IMeetingViewModel _meetingViewModel;
        private readonly int _snapToMinutes;
        private readonly bool _rightToLeft;
        public MeetingMoveState MeetingMoveState { get; set; }

        public MeetingMover(IMeetingSchedulesView schedulesView, IMeetingViewModel meetingViewModel, int snapToMinutes, bool rightToLeft)
        {
            _schedulesView = schedulesView;
            _meetingViewModel = meetingViewModel;
            _snapToMinutes = snapToMinutes;
            _rightToLeft = rightToLeft;
            MeetingMoveState = MeetingMoveState.None;
        }

        public void Move(ILengthToTimeCalculator pixelConverter, int mouseCellPosition, Rectangle cellRect, MeetingMoveState moveState, TimeSpan diffStart)
        {
            if(moveState.Equals(MeetingMoveState.MovingStart)) MoveStart(pixelConverter, mouseCellPosition, cellRect);
            if(moveState.Equals(MeetingMoveState.MovingEnd)) MoveEnd(pixelConverter, mouseCellPosition, cellRect);
            if(moveState.Equals(MeetingMoveState.MovingStartAndEnd)) MoveStartAndEnd(pixelConverter, mouseCellPosition, cellRect, diffStart);
        }

        private void MoveStart(ILengthToTimeCalculator pixelConverter, int mouseCellPosition, Rectangle cellRect)
        {
            if(pixelConverter == null) throw new ArgumentNullException(nameof(pixelConverter));

            var refresh = false;

            if (mouseCellPosition < 0) mouseCellPosition = 0;
            if (mouseCellPosition > cellRect.Width) mouseCellPosition = cellRect.Width;

            var dateTime = TimeZoneHelper.ConvertFromUtc(pixelConverter.DateTimeFromPosition(mouseCellPosition, _rightToLeft), TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
            if (dateTime.Date > _meetingViewModel.StartDate.Date)
            {
                dateTime = _meetingViewModel.StartDate.Date.AddDays(1);
                dateTime = dateTime.Subtract(TimeSpan.FromMinutes(_snapToMinutes));
            }

            var snappedTime = GetSnappedTime(dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond)).TimeOfDay);

            if (snappedTime >= TimeSpan.FromDays(1)) snappedTime = TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(_snapToMinutes));
            
            if (snappedTime.Add(TimeSpan.FromMinutes(_snapToMinutes)) >= _meetingViewModel.EndTime && _meetingViewModel.StartDate.Equals(_meetingViewModel.EndDate))
            {
                if (_meetingViewModel.StartTime != _meetingViewModel.EndTime.Subtract(TimeSpan.FromMinutes(_snapToMinutes)))
                {
                    _meetingViewModel.StartTime = _meetingViewModel.EndTime.Subtract(TimeSpan.FromMinutes(_snapToMinutes));
                    refresh = true;
                }
            }
            else
            {
                if (_meetingViewModel.StartTime != snappedTime)
                {
                    _meetingViewModel.StartTime = snappedTime;
                    refresh = true;
                }
            }

            if (refresh) _schedulesView.RefreshGridSchedules();
        }

        private void MoveEnd(ILengthToTimeCalculator pixelConverter, int mouseCellPosition, Rectangle cellRect)
        {
            if(pixelConverter == null) throw new ArgumentNullException(nameof(pixelConverter));

            var refresh = false;

            if (mouseCellPosition > cellRect.Width) mouseCellPosition = cellRect.Width;
            if (mouseCellPosition < 0) mouseCellPosition = 0;

            var dateTime = TimeZoneHelper.ConvertFromUtc(pixelConverter.DateTimeFromPosition(mouseCellPosition, _rightToLeft), TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
            var snappedTime = GetSnappedTime(dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond)).TimeOfDay);

            if (snappedTime.Subtract(TimeSpan.FromMinutes(_snapToMinutes)) <= _meetingViewModel.StartTime && !(dateTime.Date > _meetingViewModel.StartDate.Date))
            {
                if (_meetingViewModel.EndTime != _meetingViewModel.StartTime.Add(TimeSpan.FromMinutes(_snapToMinutes)))
                {
                    _meetingViewModel.EndTime = _meetingViewModel.StartTime.Add(TimeSpan.FromMinutes(_snapToMinutes));
                    refresh = true;
                }
            }
            else
            {
                if (_meetingViewModel.EndTime != snappedTime)
                {
                    _meetingViewModel.EndTime = snappedTime;
                    refresh = true;
                }
            }

            if (refresh) _schedulesView.RefreshGridSchedules();
        }

        private void MoveStartAndEnd(ILengthToTimeCalculator pixelConverter, int mouseCellPosition, Rectangle cellRect, TimeSpan diffStart)
        {
            if(pixelConverter == null) throw new ArgumentNullException(nameof(pixelConverter));

            var refresh = false;

            if (mouseCellPosition > cellRect.Width) mouseCellPosition = cellRect.Width;
            if (mouseCellPosition < 0) mouseCellPosition = 0;

            var dateTime = TimeZoneHelper.ConvertFromUtc(pixelConverter.DateTimeFromPosition(mouseCellPosition, _rightToLeft), TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
            var time = dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));

            if (diffStart < TimeSpan.Zero) diffStart = TimeSpan.FromDays(1).Add(diffStart);
            var adjustedTime = time.Subtract(diffStart);

            if (adjustedTime.Date < _meetingViewModel.StartDate.Date) adjustedTime = _meetingViewModel.StartDate.Date;
            var snappedStart = GetSnappedTime(adjustedTime.TimeOfDay);
            if (adjustedTime.Date > _meetingViewModel.StartDate.Date) snappedStart = snappedStart.Add(TimeSpan.FromDays(1));
            var duration = _meetingViewModel.MeetingDuration;
            if (snappedStart < TimeSpan.Zero) snappedStart = TimeSpan.Zero;

            var maxPos = cellRect.Width;
            if(_rightToLeft) maxPos = 0;

	        var fromUtc = TimeZoneHelper.ConvertFromUtc(pixelConverter.DateTimeFromPosition(maxPos, _rightToLeft), TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone);
	        if (_meetingViewModel.StartDate.Date.Add(snappedStart.Add(duration)) > fromUtc)
            {
                var maxStart = fromUtc.Subtract(duration);
                if (!maxStart.Date.Equals(_meetingViewModel.StartDate.Date)) snappedStart = TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(_snapToMinutes));
                else snappedStart = maxStart.TimeOfDay;
            }

            if (snappedStart >= TimeSpan.FromDays(1)) snappedStart = TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(_snapToMinutes));

            if (_meetingViewModel.StartTime != snappedStart)
            {
                _meetingViewModel.StartTime = snappedStart;
                _meetingViewModel.EndTime = snappedStart.Add(duration);
                refresh = true;
            }

            if (refresh) _schedulesView.RefreshGridSchedules();
        }

        private TimeSpan GetSnappedTime(TimeSpan timeSpan)
        {
            var remainder = timeSpan.TotalMinutes % _snapToMinutes;

            if (remainder == 0)
                return timeSpan;

            if (remainder < (Double)_snapToMinutes / 2)
                return timeSpan.Subtract(TimeSpan.FromMinutes(remainder));

            return timeSpan.Add(TimeSpan.FromMinutes(_snapToMinutes - remainder));
        }
    }
}
