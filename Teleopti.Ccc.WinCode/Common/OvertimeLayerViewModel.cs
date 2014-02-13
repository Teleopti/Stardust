using System.Linq;
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
	    private readonly IMoveShiftLayerVertical _moveShiftLayerVertical;

	    public OvertimeLayerViewModel(IVisualLayer layer)
            : base(layer)
        {
        }

     
        public OvertimeLayerViewModel(ILayerViewModelObserver observer, IOvertimeShiftLayer layer, IPersonAssignment assignment, IEventAggregator eventAggregator, IMoveShiftLayerVertical moveShiftLayerVertical)
            : base(observer, layer, assignment, eventAggregator, moveShiftLayerVertical)
        {
	        _layer = layer;
	        _assignment = assignment;
	        _moveShiftLayerVertical = moveShiftLayerVertical;
        }

	    public override bool CanMoveUp
	    {
				get { return _moveShiftLayerVertical != null && _assignment != null && _assignment.OvertimeActivities().ToList().IndexOf(_layer) > 0; ; }
	    }

	    public override bool CanMoveDown
	    {
		    get
		    {
			    return _moveShiftLayerVertical != null && !isLayerLast();
		    }
	    }

		private bool isLayerLast()
		{
			var overtimeLayers = _assignment.OvertimeActivities().ToList();
			return overtimeLayers.Any() && overtimeLayers.Last().Equals(_layer);
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
			
				public override int VisualOrderIndex
				{
					get { return 100 + _assignment.OvertimeActivities().ToList().IndexOf(_layer); ; }
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