using System;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
 
    public interface IMeetingImpactTransparentWindowHandler
    {
        event EventHandler<EventArgs> TransparentWindowMoved;
        void DrawMeeting(TimeSpan meetingStart, TimeSpan meetingEnd);
        void ScrollMeetingIntoView();
    }

    public class MeetingImpactTransparentWindowHandler : IMeetingImpactTransparentWindowHandler
    {
        private TransparentControlMeetingHelper _transparentControlHelper;
		private TransparentMeetingControlModel _transparentMeetingControlModel;
        private readonly IMeetingImpactView _meetingImpactView;
        private readonly IMeetingViewModel _meetingViewModel;
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;


        public event EventHandler<EventArgs> TransparentWindowMoved;

        public MeetingImpactTransparentWindowHandler(IMeetingImpactView meetingImpactView, IMeetingViewModel meetingViewModel, 
            ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _meetingImpactView = meetingImpactView;
            _meetingViewModel = meetingViewModel;
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }
        public void DrawMeeting(TimeSpan meetingStart, TimeSpan meetingEnd)
        {
            _transparentControlHelper = CreateHelper();

            if (_transparentControlHelper == null)
                return;

            int leftPos = _transparentControlHelper.GetPositionFromTimeSpan(meetingStart);
            int rightPos = _transparentControlHelper.GetPositionFromTimeSpan(meetingEnd);

            if (_transparentMeetingControlModel != null)
                _transparentMeetingControlModel.TransparentControlModelChanged -= TransparentControlModelTransparentControlModelChanged;
             
            if (!_meetingImpactView.IsRightToLeft)
                _transparentMeetingControlModel = new TransparentMeetingControlModel(leftPos, rightPos - leftPos, _meetingImpactView.ResultGrid, Color.Gray, 50);
            else
                _transparentMeetingControlModel = new TransparentMeetingControlModel(rightPos, leftPos - rightPos, _meetingImpactView.ResultGrid, Color.Gray, 50);

            _transparentMeetingControlModel.TransparentControlModelChanged += TransparentControlModelTransparentControlModelChanged;

            _transparentMeetingControlModel.Height = _meetingImpactView.RowsHeight;
            _transparentMeetingControlModel.Top = _meetingImpactView.ClientRectangleTop + _meetingImpactView.RowHeaderHeight;
            _transparentMeetingControlModel.BorderWidth = 2;

            _meetingImpactView.ShowMeeting(_transparentMeetingControlModel, _transparentControlHelper);
        }

        
        public void ScrollMeetingIntoView()
        {
            if (_transparentControlHelper == null)
                return;
            _transparentControlHelper.ScrollAdjust = 0;
            var col = _meetingImpactView.GridColCount;
            
            if (_meetingViewModel.EndDate == _meetingViewModel.StartDate)
            {
                col = _transparentControlHelper.GetColumnFromTimeSpan(_meetingViewModel.EndTime, _meetingImpactView.ColsHeaderWidth, _meetingImpactView.SelectedSkill().DefaultResolution);
            }

            _meetingImpactView.ScrollMeetingIntoView(col);
            _meetingImpactView.RefreshMeetingControl();
       }

        private TransparentControlMeetingHelper CreateHelper()
        {
            if (!_meetingImpactView.HasStartInterval())
                return null;

            var startValue = _meetingImpactView.IntervalStartValue();

            var skill = _meetingImpactView.SelectedSkill();
			if (skill == null) return null;
            var minutes = (_meetingImpactView.GridColCount - 1) * skill.DefaultResolution + skill.DefaultResolution;
            var endValue = startValue.Add(TimeSpan.FromMinutes(minutes));

            int leftBorder;
            int rigthBorder;
            int minTimePos;
            int maxTimePos;
            int scrollAdjust;

            var rightToLeft = _meetingImpactView.IsRightToLeft;
            if (!rightToLeft)
            {
                leftBorder = _meetingImpactView.ClientRectangleLeft + _meetingImpactView.ColsHeaderWidth;
                rigthBorder = _meetingImpactView.ClientRectangleRight;
                minTimePos = leftBorder;
                maxTimePos = _meetingImpactView.IntervalsTotalWidth + _meetingImpactView.ColsHeaderWidth;
                scrollAdjust = HScrollValue() - _meetingImpactView.ColsHeaderWidth;
            }
            else
            {
                leftBorder = _meetingImpactView.ClientRectangleLeft;
                rigthBorder = _meetingImpactView.ClientRectangleRight - _meetingImpactView.ColsHeaderWidth;
                minTimePos = leftBorder;
                maxTimePos = _meetingImpactView.IntervalsTotalWidth;
                scrollAdjust = _meetingImpactView.ColsHeaderWidth - HScrollValue();
            }

            //happens sometimes??
            if (rigthBorder < leftBorder)
                rigthBorder = leftBorder;

            var minMaxBorders = new MinMax<int>(leftBorder, rigthBorder);
            var minMaxTime = new MinMax<TimeSpan>(startValue, endValue);
            var minMaxTimePos = new MinMax<int>(minTimePos, maxTimePos);

            return new TransparentControlMeetingHelper(minMaxBorders, minMaxTime, minMaxTimePos, LowestResolution(skill.DefaultResolution)) { IsRightToLeft = rightToLeft, ScrollAdjust = scrollAdjust };
        }

        public int LowestResolution(int selectedSkillResolution)
        {
	        var skills = _schedulingResultStateHolder.Skills;
	        if (!skills.Any()) return selectedSkillResolution;

            var min = (from r in skills
                       select r.DefaultResolution).Min();

            return min;
        }

        private void TransparentControlModelTransparentControlModelChanged(object sender, EventArgs e)
        {
            TimeSpan start;
            TimeSpan end;

            try
            {
                if (!_meetingImpactView.IsRightToLeft)
                {
                    start = _transparentControlHelper.GetTimeSpanFromPosition(_transparentMeetingControlModel.Left + HScrollValue() - _meetingImpactView.ColsHeaderWidth);
                    end = _transparentControlHelper.GetTimeSpanFromPosition(_transparentMeetingControlModel.Left + _transparentMeetingControlModel.Width + HScrollValue() - _meetingImpactView.ColsHeaderWidth);
                }
                else
                {
                    start = _transparentControlHelper.GetTimeSpanFromPosition(_transparentMeetingControlModel.Left + _transparentMeetingControlModel.Width - HScrollValue() + _meetingImpactView.ColsHeaderWidth);
                    end = _transparentControlHelper.GetTimeSpanFromPosition(_transparentMeetingControlModel.Left - HScrollValue() + _meetingImpactView.ColsHeaderWidth);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                _transparentMeetingControlModel.Left = _transparentControlHelper.GetPositionFromTimeSpan(_meetingViewModel.StartTime);
                start = _meetingViewModel.StartTime;
                end = _meetingViewModel.EndTime;
            }

            _meetingViewModel.StartTime = start;
            _meetingViewModel.EndTime = end;

            OnTransparentWindowMoved(EventArgs.Empty);

        }

        private void OnTransparentWindowMoved(EventArgs e)
        {
	        var onTransparentWindowMoved = TransparentWindowMoved;
	        if (onTransparentWindowMoved != null)
            {
                onTransparentWindowMoved(this, e);
            }
        }

	    private int HScrollValue()
        {
            return _meetingImpactView.GetCurrentHScrollPixelPos;
        }
    }

}