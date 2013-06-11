using System;
using Microsoft.Practices.Composite.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class MeetingLayerViewModel : LayerViewModel
    {
        private IPersonMeeting _meeting;

        public MeetingLayerViewModel(ILayer layer, IEventAggregator eventAggregator)
            : this(layer, null, eventAggregator)
        {
        }

        public MeetingLayerViewModel(ILayer layer, IShift parent, IEventAggregator eventAggregator)
					: base(null, layer, parent, eventAggregator)
        {
        }

        public MeetingLayerViewModel(ILayerViewModelObserver observer, ILayer layer, IEventAggregator eventAggregator)
            : base(observer,layer, null,eventAggregator)
        {
        }

        public MeetingLayerViewModel(ILayerViewModelObserver observer, IPersonMeeting meeting, IEventAggregator eventAggregator)
            : base(observer, meeting.ToLayer(), null,eventAggregator)
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