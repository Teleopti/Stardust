using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class MeetingLayerViewModel : LayerViewModel
    {
        private readonly IPersonMeeting _meeting;

        public MeetingLayerViewModel(ILayerViewModelObserver observer, IPersonMeeting meeting, IEventAggregator eventAggregator)
            : base(observer, meeting.ToLayer(), eventAggregator, false, meeting.Person)
        {
            _meeting = meeting;
        }

        public override string LayerDescription
        {
            get { return UserTexts.Resources.Meeting; }
        }
			
	    public override int VisualOrderIndex
	    {
				get { return 301; }
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