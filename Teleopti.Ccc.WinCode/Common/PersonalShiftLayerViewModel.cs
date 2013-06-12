using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class PersonalShiftLayerViewModel : LayerViewModel
    {
        public PersonalShiftLayerViewModel(ILayerViewModelObserver observer, ILayer<IActivity> layer, IShift parent, IEventAggregator eventAggregator)
            : base(observer,layer, parent,eventAggregator, false)
        {
        }


        public override string LayerDescription
        {
            get { return UserTexts.Resources.PersonalShifts; }
        }

        protected override int OrderIndexBase
        {
            get
            {
                int idx = 0;
                var parent = Parent as Domain.Scheduling.Assignment.PersonalShift;
                if (parent != null)
                    idx = parent.OrderIndex;
                return 200 + idx;
            }
        }

        public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

        //TODO: Make sure how the vertical movement of personalshift should work
        public override bool CanMoveUp
        {
            get { return false;}
        }

        //TODO: Make sure how the vertical movement of personalshift should work
        public override bool CanMoveDown
        {
            get { return false;}
        }

        public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return false;
        }
    }
}