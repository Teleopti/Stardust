using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class OvertimeLayerViewModel : MoveableLayerViewModel
    {
        public OvertimeLayerViewModel(ILayer layer,IEventAggregator eventAggregator)
            : this(layer,null,eventAggregator)
        {
            
        }

        public OvertimeLayerViewModel(ILayer layer, IShift parent, IEventAggregator eventAggregator)
            : base(layer, parent, eventAggregator)
        {
        }

        public OvertimeLayerViewModel(ILayerViewModelObserver observer, ILayer layer, IEventAggregator eventAggregator)
            : base(observer, layer, null, eventAggregator)
        {
           
        }

        public override bool Opaque
        {
            get{ return true; }
        }

        public override string LayerDescription
        {
            get {
                IOvertimeShiftActivityLayer overtimeShiftActivityLayer = Layer as IOvertimeShiftActivityLayer;
                if (overtimeShiftActivityLayer != null)
                    return overtimeShiftActivityLayer.DefinitionSet.Name;
                
                return UserTexts.Resources.Overtime; 
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
                return TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

        public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return sender != this && !IsProjectionLayer && ((sender.GetType() == typeof(MainShiftLayerViewModel)) || (sender.GetType() == typeof(OvertimeLayerViewModel)));
        }
    }
}