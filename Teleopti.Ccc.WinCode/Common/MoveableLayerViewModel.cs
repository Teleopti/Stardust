using Microsoft.Practices.Composite.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Baseclass for viewmodels that can be moved  vertically
    /// </summary>
    public abstract class MoveableLayerViewModel : LayerViewModel
    {
        protected MoveableLayerViewModel(ILayer layer, IShift parent,IEventAggregator eventAggregator)
            : base(layer, parent, eventAggregator)
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
                Parent.LayerCollection.MoveDownLayer(Layer as ILayer<IActivity>);
                LayerMoved();
            }

        }

        public override void MoveUp()
        {
            if (CanMoveUp)
            {
                Parent.LayerCollection.MoveUpLayer(Layer as ILayer<IActivity>);
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