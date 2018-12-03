using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingImpactTransparentWindowHandlerTest
    {
        private MockRepository _mocks;
        private IMeetingImpactView _meetingImpactView;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IMeetingViewModel _meetingViewModel;
        private ISkill _skill;
        private ISkill[] _skills;
        private MeetingImpactTransparentWindowHandler _target;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _meetingImpactView = _mocks.StrictMock<IMeetingImpactView>();
            _meetingViewModel = _mocks.StrictMock<IMeetingViewModel>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _skill = _mocks.StrictMock<ISkill>();
            _skills = new [] { _skill };
            _target = new MeetingImpactTransparentWindowHandler(_meetingImpactView,_meetingViewModel,_schedulingResultStateHolder);
        }

        [Test]
        public void ShouldDrawMeetingOnLeftToRight()
        {
            Expect.Call(_meetingImpactView.IsRightToLeft).Return(false).Repeat.Twice();

            Expect.Call(_meetingImpactView.HasStartInterval()).Return(true);
            Expect.Call(_meetingImpactView.IntervalStartValue()).Return(TimeSpan.FromHours(8));
            Expect.Call(_meetingImpactView.SelectedSkill()).Return(_skill);
            Expect.Call(_meetingImpactView.GridColCount).Return(24).Repeat.Times(1);
            Expect.Call(_skill.DefaultResolution).Return(15).Repeat.Times(4);
            Expect.Call(_meetingImpactView.ClientRectangleLeft).Return(0);
            Expect.Call(_meetingImpactView.ColsHeaderWidth).Return(15).Repeat.Times(3);
            Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
            Expect.Call(_meetingImpactView.ClientRectangleRight).Return(200);
            Expect.Call(_meetingImpactView.IntervalsTotalWidth).Return((200));
            Expect.Call(_meetingImpactView.GetCurrentHScrollPixelPos).Return(0);
            Expect.Call(_meetingImpactView.ResultGrid).Return(null);
            Expect.Call(_meetingImpactView.RowsHeight).Return(200);
            Expect.Call(_meetingImpactView.ClientRectangleTop).Return(0);
            Expect.Call(_meetingImpactView.RowHeaderHeight).Return(10);
            Expect.Call(() => _meetingImpactView.ShowMeeting(null, null)).IgnoreArguments();

            var startDate = new DateOnly(2010, 11, 1);
            Expect.Call(_meetingViewModel.EndTime).Return(TimeSpan.FromHours(12));
            Expect.Call(_meetingViewModel.EndDate).Return(startDate);
            Expect.Call(_meetingViewModel.StartDate).Return(startDate);
            Expect.Call(_meetingImpactView.GridColCount).Return(24);

            Expect.Call(_meetingImpactView.SelectedSkill()).Return(_skill);
            Expect.Call(_skill.DefaultResolution).Return(15);
            Expect.Call(_meetingImpactView.ColsHeaderWidth).Return(15);
            Expect.Call(() => _meetingImpactView.ScrollMeetingIntoView(12)).IgnoreArguments();
            Expect.Call(() => _meetingImpactView.RefreshMeetingControl());

            _mocks.ReplayAll();
            _target.DrawMeeting(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
            _target.ScrollMeetingIntoView();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldDrawMeetingOnRightToLeft()
        {

            Expect.Call(_meetingImpactView.IsRightToLeft).Return(true).Repeat.Twice();

            Expect.Call(_meetingImpactView.HasStartInterval()).Return(true);
            Expect.Call(_meetingImpactView.IntervalStartValue()).Return(TimeSpan.FromHours(8));
            Expect.Call(_meetingImpactView.SelectedSkill()).Return(_skill);
            Expect.Call(_meetingImpactView.GridColCount).Return(24).Repeat.Times(1);
            Expect.Call(_skill.DefaultResolution).Return(15).Repeat.Times(4);
            Expect.Call(_meetingImpactView.ClientRectangleLeft).Return(0);
            Expect.Call(_meetingImpactView.ColsHeaderWidth).Return(15).Repeat.Times(2);
            Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
            Expect.Call(_meetingImpactView.ClientRectangleRight).Return(200);
            Expect.Call(_meetingImpactView.IntervalsTotalWidth).Return((200));
            Expect.Call(_meetingImpactView.GetCurrentHScrollPixelPos).Return(0);
            Expect.Call(_meetingImpactView.ResultGrid).Return(null);
            Expect.Call(_meetingImpactView.RowsHeight).Return(200);
            Expect.Call(_meetingImpactView.ClientRectangleTop).Return(0);
            Expect.Call(_meetingImpactView.RowHeaderHeight).Return(10);
            Expect.Call(() => _meetingImpactView.ShowMeeting(null, null)).IgnoreArguments();

            _mocks.ReplayAll();
            _target.DrawMeeting(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldJumpOutIfTryingToScrollToEarly()
        {
            _target.ScrollMeetingIntoView();
        }

        [Test]
        public void ShouldNotDrawIfNoStartInterval()
        {
            Expect.Call(_meetingImpactView.HasStartInterval()).Return(false);
            _mocks.ReplayAll();
            _target.DrawMeeting(TimeSpan.FromHours(11),TimeSpan.FromHours(12));
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldReturnSuppliedSkillResolutionIfMinNotFound()
		{
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Skills).Return(new ISkill[]{});
			}

			using (_mocks.Playback())
			{
				var resolution = _target.LowestResolution(30);
				Assert.AreEqual(30, resolution);
			}
		}

	    [Test]
	    public void ShouldReturnLowestResolution()
	    {
		    using (_mocks.Record())
		    {
			    Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);
			    Expect.Call(_skill.DefaultResolution).Return(15);
		    }

		    using (_mocks.Playback())
		    {
			    var resolution = _target.LowestResolution(30);
			    Assert.AreEqual(15, resolution);
		    }
	    }

    }

}