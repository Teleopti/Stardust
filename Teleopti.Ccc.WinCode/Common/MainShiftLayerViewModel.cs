using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class MainShiftLayerViewModel : MoveableLayerViewModel
    {
	    private readonly IMainShiftLayer _layer;
	    private readonly IPersonAssignment _assignment;
	    private readonly IMoveLayerVertical _moveLayer;

	    public MainShiftLayerViewModel(IVisualLayer layer)
            : base(layer)
        {
        }

        public MainShiftLayerViewModel(ILayerViewModelObserver observer, IMainShiftLayer layer, IPersonAssignment assignment, IEventAggregator eventAggregator, IMoveLayerVertical moveLayer)
				: base(observer, layer, assignment, eventAggregator, moveLayer)
        {
	        _layer = layer;
	        _assignment = assignment;
	        _moveLayer = moveLayer;
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
                return PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

	    public override bool CanMoveUp
	    {
				get { return _moveLayer!=null && _assignment.MainActivities().ToList().IndexOf(_layer) > 0; }
	    }

	    public override bool CanMoveDown
	    {
		    get
		    {
			    return _moveLayer != null && !isLayerLast();
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