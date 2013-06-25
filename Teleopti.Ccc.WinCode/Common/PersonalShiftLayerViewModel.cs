using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Editor;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class PersonalShiftLayerViewModel : LayerViewModel
    {
	    private readonly IPersonalShiftLayer _layer;
	    private readonly IPersonAssignment _parent;
	    private readonly IMoveLayerVertical _moveLayerVertical;

	    public PersonalShiftLayerViewModel(ILayerViewModelObserver observer, IPersonalShiftLayer layer, IPersonAssignment parent, IEventAggregator eventAggregator, IMoveLayerVertical moveLayerVertical)
            : base(observer,layer, eventAggregator, false)
	    {
		    _layer = layer;
		    _parent = parent;
				_moveLayerVertical = moveLayerVertical;
	    }


	    public override string LayerDescription
        {
            get { return UserTexts.Resources.PersonalShifts; }
        }

	    protected override int OrderIndexBase
        {
            get
            {
                var idx = 0;
                if (_parent != null)
									idx = _parent.PersonalLayers.ToList().IndexOf(_layer);
                return 200 + idx;
            }
        }

        public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

        public override bool CanMoveUp
        {
            get
            {
				if (_parent != null)
				{
					return _layer.OrderIndex > 0;
				}

	            return false;
            }
        }

        public override bool CanMoveDown
        {
			get
			{
				if (_parent != null)
				{
					return _parent.PersonalLayers.Contains(_layer) && !_parent.PersonalLayers.ToList().Last().Equals(_layer);
				}

				return false;
			}
        }

		public override void MoveDown()
		{
			if (CanMoveDown)
			{
				_moveLayerVertical.MoveDown(_parent, _layer);
				LayerMoved();
			}
		}

		public override void MoveUp()
		{
			if (CanMoveUp)
			{
				_moveLayerVertical.MoveUp(_parent, _layer);
				LayerMoved();
			}
		}

        public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return false;
        }

				protected override void Replace()
				{
					if (ParentObservingCollection != null)
						ParentObservingCollection.ReplaceActivity(this, _layer, SchedulePart);
				}

		private void LayerMoved()
		{
			if (ParentObservingCollection != null)
			{
				ParentObservingCollection.LayerMovedVertically(this);
				new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
			}
		}
    }
}