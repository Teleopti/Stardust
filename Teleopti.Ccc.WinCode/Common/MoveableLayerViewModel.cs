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
	    private readonly IShift _parent;

	    protected MoveableLayerViewModel(ILayer<IPayload> layer)
						: base(null, layer, null, true)
        {
        }

        protected MoveableLayerViewModel(ILayerViewModelObserver observer, ILayer<IActivity> layer, IShift parent,IEventAggregator eventAggregator)
            : base(observer, layer, eventAggregator, false)
        {
	        _layer = layer;
	        _parent = parent;
        }

	    public override bool CanMoveUp
        {
            get
            {
                return _parent != null && IsMovePermitted() && _parent.LayerCollection.CanMoveUpLayer(_layer);
            }
        }

        public override bool CanMoveDown
        {
            get
            {
                return _parent != null && IsMovePermitted() && _parent.LayerCollection.CanMoveDownLayer(_layer);
            }
        }


        public override void MoveDown()
        {
            if (CanMoveDown)
            {
	            _parent.LayerCollection.MoveDownLayer(_layer);
                LayerMoved();
            }

        }

        public override void MoveUp()
        {
            if (CanMoveUp)
            {
	            _parent.LayerCollection.MoveUpLayer(_layer);
				LayerMoved();
            }
        }

        private void LayerMoved()
        {
            if (ParentObservingCollection != null)
            {
                ParentObservingCollection.LayerMovedVertically(this);
                SynchronizeWithDomainRoger();
            }
        }

       
    }
}