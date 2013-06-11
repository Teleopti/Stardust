using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class MainShiftLayerViewModel : MoveableLayerViewModel
    {
        public MainShiftLayerViewModel(ILayer layer, IEventAggregator eventAggregator)
            : base(layer,null,eventAggregator)
        {
        }

        public MainShiftLayerViewModel(ILayerViewModelObserver observer, ILayer layer, IShift parent, IEventAggregator eventAggregator)
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
    }
}