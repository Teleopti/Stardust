using Microsoft.Practices.Composite.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Baseclass for viewmodels that can be moved  vertically
    /// </summary>
    public abstract class MoveableLayerViewModel : LayerViewModel
    {
	    private readonly ILayer<IActivity> _layer;

        protected MoveableLayerViewModel(ILayer<IPayload> layer)
						: base(null, layer, null, null, true)
        {
        }

        protected MoveableLayerViewModel(ILayerViewModelObserver observer, ILayer<IActivity> layer, IShift parent,IEventAggregator eventAggregator)
            : base(observer, layer, parent,eventAggregator, false)
        {
	        _layer = layer;
        }

        public override bool CanMoveUp
        {
            get
            {
                return Parent != null && IsMovePermitted() && Parent.LayerCollection.CanMoveUpLayer(_layer);
            }
        }

        public override bool CanMoveDown
        {
            get
            {
                return Parent != null && IsMovePermitted() && Parent.LayerCollection.CanMoveDownLayer(_layer);
            }
        }


        public override void MoveDown()
        {
            if (CanMoveDown)
            {
	            Parent.LayerCollection.MoveDownLayer(_layer);
                LayerMoved();
            }

        }

        public override void MoveUp()
        {
            if (CanMoveUp)
            {
	            Parent.LayerCollection.MoveUpLayer(_layer);
				LayerMoved();
            }
        }

        private void LayerMoved()
        {
            if (ParentObservingCollection != null)
            {
                ParentObservingCollection.LayerMovedVertically(this);
                TriggerShiftEditorUpdate();
            }
        }

       
    }
}