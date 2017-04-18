using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class MainShiftLayerViewModel : MoveableLayerViewModel
    {
	    private readonly MainShiftLayer _layer;
	    private readonly IPersonAssignment _assignment;

	    public MainShiftLayerViewModel(IVisualLayer layer)
            : base(layer)
        {
        }

        public MainShiftLayerViewModel(ILayerViewModelObserver observer, MainShiftLayer layer, IPersonAssignment assignment, IEventAggregator eventAggregator)
				: base(observer, layer, assignment, eventAggregator)
        {
	        _layer = layer;
	        _assignment = assignment;
        }


	    public override string LayerDescription
        {
            get { return UserTexts.Resources.Activity; }
        }


				public override int VisualOrderIndex
				{
					get { return _assignment.MainActivities().ToList().IndexOf(_layer); }
				}

	    public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

	    public override bool CanMoveUp
	    {
				get { return _assignment != null && _assignment.MainActivities().ToList().IndexOf(_layer) > 0; }
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
			var mainLayers = _assignment.MainActivities().ToList();
			return mainLayers.Any() && mainLayers.Last().Equals(_layer);
		}

		protected override void Replace()
		{
			if (ParentObservingCollection != null)  ParentObservingCollection.ReplaceActivity(this, Layer as ILayer<IActivity>, SchedulePart);
		}

    }
}