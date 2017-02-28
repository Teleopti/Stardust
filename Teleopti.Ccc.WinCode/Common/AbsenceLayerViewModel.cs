using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Editor;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class AbsenceLayerViewModel : LayerViewModel
    {
	    public AbsenceLayerViewModel(ILayerViewModelObserver observer, ILayer<IAbsence> layer, IEventAggregator eventAggregator)
				: base(observer, layer, eventAggregator, false)
			{
			}

			public AbsenceLayerViewModel(IVisualLayer layer)
				: base(null, layer, null, true)
			{
			}

        public override string LayerDescription
        {
            get { return UserTexts.Resources.Absence; }
        }

        public override int VisualOrderIndex
        {
            get
            {
                return 400;
            }
        }

        public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);
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
                ParentObservingCollection.RemoveAbsence(this,Layer as ILayer<IAbsence>,SchedulePart);
				new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
            }
        }

		protected override void Replace()
		{
			if (ParentObservingCollection!=null) ParentObservingCollection.ReplaceAbsence(this, Layer as ILayer<IAbsence>, SchedulePart);
		}
    }
}