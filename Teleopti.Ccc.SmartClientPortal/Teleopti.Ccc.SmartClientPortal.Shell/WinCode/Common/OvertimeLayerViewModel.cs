using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class OvertimeLayerViewModel : MoveableLayerViewModel
    {
	    private readonly OvertimeShiftLayer _layer;
	    private readonly IPersonAssignment _assignment;
		private readonly IAuthorization _authorization;

		public OvertimeLayerViewModel(IVisualLayer layer, IPerson person, IAuthorization authorization = null)
            : base(layer, person)
		{
			_authorization = authorization ?? PrincipalAuthorization.Current_DONTUSE();
		}
		
        public OvertimeLayerViewModel(ILayerViewModelObserver observer, OvertimeShiftLayer layer, IPersonAssignment assignment, IEventAggregator eventAggregator, IAuthorization authorization = null)
            : base(observer, layer, assignment, eventAggregator)
        {
	        _layer = layer;
	        _assignment = assignment;
			_authorization = authorization ?? PrincipalAuthorization.Current_DONTUSE();
		}

	    public override bool CanMoveUp => _assignment != null && _assignment.OvertimeActivities().ToList().IndexOf(_layer) > 0;

		public override bool CanMoveDown => _assignment != null && !isLayerLast();

		private bool isLayerLast()
		{
			var overtimeLayers = _assignment.OvertimeActivities().ToList();
			return overtimeLayers.Any() && overtimeLayers.Last().Equals(_layer);
		}

	    public override bool Opaque => true;

		public override string LayerDescription => _layer != null ? _layer.DefinitionSet.Name : UserTexts.Resources.Overtime;

		public override int VisualOrderIndex => 100 + _assignment.OvertimeActivities().ToList().IndexOf(_layer);

		public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

        public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return sender != this && !IsProjectionLayer && (sender.GetType() == typeof(MainShiftLayerViewModel) || sender.GetType() == typeof(OvertimeLayerViewModel));
        }

		protected override void Replace()
		{
			ParentObservingCollection?.ReplaceActivity(this,Layer as ILayer<IActivity>,SchedulePart);
		}
    }
}