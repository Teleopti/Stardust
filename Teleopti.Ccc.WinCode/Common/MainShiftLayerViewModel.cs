using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class MainShiftLayerViewModel : MoveableLayerViewModel
    {
	    public MainShiftLayerViewModel(IVisualLayer layer)
            : base(layer)
        {
        }

        public MainShiftLayerViewModel(ILayerViewModelObserver observer, ILayer<IActivity> layer, IShift parent, IEventAggregator eventAggregator)
            : base(observer,layer, parent, eventAggregator)
        {
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

		protected override void DeleteLayer()
		{
			if (ParentObservingCollection != null)
			{
				ParentObservingCollection.RemoveActivity(this,Layer as ILayer<IActivity>,SchedulePart);
				TriggerShiftEditorUpdate();
			}
		}
    }
}