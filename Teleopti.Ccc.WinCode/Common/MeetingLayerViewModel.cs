using Microsoft.Practices.Composite.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class MeetingLayerViewModel : LayerViewModel
    {
        private readonly IPersonMeeting _meeting;

        public MeetingLayerViewModel(ILayerViewModelObserver observer, IPersonMeeting meeting, IEventAggregator eventAggregator)
            : base(observer, meeting.ToLayer(), eventAggregator, false)
        {
            _meeting = meeting;
        }

        public override string LayerDescription
        {
            get { return UserTexts.Resources.Meeting; }
        }

        protected override int OrderIndexBase
        {
            get { return 300; }
        }
		
        public override bool IsMovePermitted()
        {
            return false;
        }

        public override bool CanMoveUp
        {
            get { return false;}
        }

        public override bool CanMoveDown
        {
            get { return false; }
        }

        public IPersonMeeting PersonMeeting
        {
            get { return _meeting; }
        }

        public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return false;
        }
    }
}