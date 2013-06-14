using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Editor;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class MainShiftLayerViewModel : LayerViewModel
    {
	    private readonly IMainShiftActivityLayerNew _layer;
	    private readonly IPersonAssignment _parent;

	    public MainShiftLayerViewModel(IVisualLayer layer)
            : base(null, layer, null, true)
        {
        }

        public MainShiftLayerViewModel(ILayerViewModelObserver observer, IMainShiftActivityLayerNew layer, IPersonAssignment parent, IEventAggregator eventAggregator)
            : base(observer,layer, eventAggregator, false)
        {
	        _layer = layer;
	        _parent = parent;
        }


	    public override string LayerDescription
        {
            get { return UserTexts.Resources.Activity; }
        }

        protected override int OrderIndexBase
        {
            get { return 0; }
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
		    get { return _layer.OrderIndex > 0; }
	    }

	    public override bool CanMoveDown
	    {
				get { return !_parent.MainShiftActivityLayers.Last().Equals(_layer); }
	    }

	    protected override void DeleteLayer()
		{
			if (ParentObservingCollection != null)
			{
				ParentObservingCollection.RemoveActivity(this,Layer as ILayer<IActivity>,SchedulePart);
				new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
			}
		}

		protected override void Replace()
		{
			if (ParentObservingCollection != null)  ParentObservingCollection.ReplaceActivity(this, Layer as ILayer<IActivity>, SchedulePart);
		}
    }
}