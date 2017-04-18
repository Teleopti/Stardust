using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class OvertimeLayerViewModel : MoveableLayerViewModel
    {
	    private readonly OvertimeShiftLayer _layer;
	    private readonly IPersonAssignment _assignment;

	    public OvertimeLayerViewModel(IVisualLayer layer)
            : base(layer)
        {
        }

     
        public OvertimeLayerViewModel(ILayerViewModelObserver observer, OvertimeShiftLayer layer, IPersonAssignment assignment, IEventAggregator eventAggregator)
            : base(observer, layer, assignment, eventAggregator)
        {
	        _layer = layer;
	        _assignment = assignment;
        }

	    public override bool CanMoveUp
	    {
				get { return _assignment != null && _assignment.OvertimeActivities().ToList().IndexOf(_layer) > 0;  }
	    }

	    public override bool CanMoveDown
	    {
		    get
		    {
			    return _assignment != null && !isLayerLast();
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
                return PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
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