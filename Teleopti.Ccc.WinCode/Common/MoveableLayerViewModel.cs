using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Editor;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Baseclass for viewmodels that can be moved  vertically
    /// </summary>
    public abstract class MoveableLayerViewModel : LayerViewModel
    {
	    private readonly ILayer<IActivity> _layer;
	    private readonly IPersonAssignment _assignment;

	    protected MoveableLayerViewModel(ILayer<IPayload> layer)
						: base(null, layer, null, true)
        {
        }

        protected MoveableLayerViewModel(ILayerViewModelObserver observer, ILayer<IActivity> layer, IPersonAssignment assignment,IEventAggregator eventAggregator)
            : base(observer, layer, eventAggregator, false)
        {
	        _layer = layer;
	        _assignment = assignment;
        }

				public override void MoveDown()
				{
					if (CanMoveDown)
					{
						_assignment.MoveLayerDown(_layer as ShiftLayer);
						LayerMoved();
					}

				}

				public override void MoveUp()
				{
					if (CanMoveUp)
					{
						_assignment.MoveLayerUp(_layer as ShiftLayer);
						LayerMoved();
					}
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