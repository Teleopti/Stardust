using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Baseclass for viewmodels that can be moved  vertically
    /// </summary>
    public abstract class MoveableLayerViewModel : LayerViewModel
    {
        protected MoveableLayerViewModel(ILayer layer, IShift parent,IEventAggregator eventAggregator)
				: base(null, layer, parent, eventAggregator)
        {
        }

        protected MoveableLayerViewModel(ILayerViewModelObserver observer, ILayer layer, IShift parent,IEventAggregator eventAggregator)
            : base(observer, layer, parent,eventAggregator)
        {
        }

        public override bool CanMoveUp
        {
            get
            {
                return Parent != null && IsMovePermitted() ?
                Parent.LayerCollection.CanMoveUpLayer(Layer as ILayer<IActivity>) : false;
            }
        }

        public override bool CanMoveDown
        {
            get
            {
                return Parent != null && IsMovePermitted() ?
                Parent.LayerCollection.CanMoveDownLayer(Layer as ILayer<IActivity>) : false;
            }
        }


        public override void MoveDown()
        {
            if (CanMoveDown)
            {
				var layer = Layer as ILayer<IActivity>;
				Parent.LayerCollection.MoveDownLayer(layer);
                LayerMoved();
            }

        }

        public override void MoveUp()
        {
            if (CanMoveUp)
            {
				var layer = Layer as ILayer<IActivity>;
				Parent.LayerCollection.MoveUpLayer(layer);
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

        protected override void DeleteLayer()
        {
            if (ParentObservingCollection != null)
            {
                ParentObservingCollection.RemoveActivity(this);
                TriggerShiftEditorUpdate();
            }
        }
    }
}