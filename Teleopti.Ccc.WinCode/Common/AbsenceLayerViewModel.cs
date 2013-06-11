using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class AbsenceLayerViewModel : LayerViewModel
    {
	    private AbsenceLayerViewModel(ILayerViewModelObserver observer, ILayer layer, IEventAggregator eventAggregator)
				: base(observer, layer, null, eventAggregator)
			{
			}
			public static AbsenceLayerViewModel CreateForProjection(ILayer layer)
			{
				return new AbsenceLayerViewModel(null, layer, null);
			}
			public static AbsenceLayerViewModel CreateForSchedule(ILayerViewModelObserver observer, ILayer<IAbsence> layer, IEventAggregator eventAggregator)
			{
				return new AbsenceLayerViewModel(observer, layer, eventAggregator);
			}


        public override string LayerDescription
        {
            get { return UserTexts.Resources.Absence; }
        }

        public override int VisualOrderIndex
        {
            get
            {
                return OrderIndexBase;
            }
        }
        protected override int OrderIndexBase
        {
            get { return 400; }
        }

        public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);
            }
            return true;
        }

        public override bool CanMoveUp
        {
            get { return false; }
        }

        public override bool CanMoveDown
        {
            get { return false;}
        }

        protected override void DeleteLayer()
        {
            if (ParentObservingCollection != null)
            {
                ParentObservingCollection.RemoveAbsence(this);
                TriggerShiftEditorUpdate();
            }
        }
    }
}