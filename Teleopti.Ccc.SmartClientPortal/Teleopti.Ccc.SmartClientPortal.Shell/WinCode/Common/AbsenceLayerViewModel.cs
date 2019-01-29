using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class AbsenceLayerViewModel : LayerViewModel
    {
		private readonly IAuthorization _authorization;

		public AbsenceLayerViewModel(ILayerViewModelObserver observer, IPersonAbsence personAbsence, IEventAggregator eventAggregator, IAuthorization authorization = null)
				: base(observer, personAbsence.Layer, eventAggregator, false, personAbsence.Person)
		{
			_authorization = authorization ?? PrincipalAuthorization.Current_DONTUSE();
		}

			public AbsenceLayerViewModel(IVisualLayer layer, IPerson person, IAuthorization authorization = null)
				: base(null, layer, null, true, person)
			{
				_authorization = authorization ?? PrincipalAuthorization.Current_DONTUSE();
			}

        public override string LayerDescription => UserTexts.Resources.Absence;

		public override int VisualOrderIndex => 400;

		public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);
            }
            return true;
        }

        public override bool CanMoveUp => false;

		public override bool CanMoveDown => false;

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
			ParentObservingCollection?.ReplaceAbsence(this, Layer as ILayer<IAbsence>, SchedulePart);
		}
    }
}