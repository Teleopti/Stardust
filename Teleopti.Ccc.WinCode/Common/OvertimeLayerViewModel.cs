using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class OvertimeLayerViewModel : MoveableLayerViewModel
    {
	    private readonly IOvertimeShiftLayer _layer;
	    private readonly IPersonAssignment _assignment;
	    private readonly IMoveLayerVertical _moveLayerVertical;
	    private IOvertimeShift _overtimeShift;

	    public OvertimeLayerViewModel(IVisualLayer layer)
            : base(layer)
        {
        }

     
        public OvertimeLayerViewModel(ILayerViewModelObserver observer, IOvertimeShiftLayer layer, IPersonAssignment assignment, IEventAggregator eventAggregator, IMoveLayerVertical moveLayerVertical)
            : base(observer, layer, assignment, eventAggregator, moveLayerVertical)
        {
	        _layer = layer;
	        _assignment = assignment;
	        _moveLayerVertical = moveLayerVertical;

	        tempFindShift();
        }

			private void tempFindShift()
			{
				//just a hack for now
				if (_assignment == null)
					return;
				foreach (var overtimeShift in _assignment.OvertimeShiftCollection)
				{
					foreach (var layer in overtimeShift.LayerCollection)
					{
						if (layer.Equals(_layer))
						{
							_overtimeShift = overtimeShift;
							return;
						}
					}
				}
			}

	    public override bool CanMoveUp
	    {
				get { return _moveLayerVertical != null && _layer.OrderIndex > 0; }
	    }

	    public override bool CanMoveDown
	    {
				get { return _moveLayerVertical != null && _overtimeShift.LayerCollection.CanMoveDownLayer(_layer); }
	    }

	    public override bool Opaque
        {
            get{ return true; }
        }

        public override string LayerDescription
        {
            get
            {
	            return _layer != null ? _layer.DefinitionSet.Name : UserTexts.Resources.Overtime;
            }
        }

        protected override int OrderIndexBase
        {
            get { return 100; }
        }

        public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

        public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return sender != this && !IsProjectionLayer && ((sender.GetType() == typeof(MainShiftLayerViewModel)) || (sender.GetType() == typeof(OvertimeLayerViewModel)));
        }

		protected override void Replace()
		{
			if(ParentObservingCollection!=null)ParentObservingCollection.ReplaceActivity(this,Layer as ILayer<IActivity>,SchedulePart);
		}
    }
}