using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class OvertimeLayerViewModel : MoveableLayerViewModel
    {
	    private readonly IOvertimeShiftActivityLayer _layer;
        public OvertimeLayerViewModel(IVisualLayer layer)
            : base(layer)
        {
        }

     
        public OvertimeLayerViewModel(ILayerViewModelObserver observer, IOvertimeShiftActivityLayer layer, IEventAggregator eventAggregator)
            : base(observer, layer, null, eventAggregator)
        {
	        _layer = layer;
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