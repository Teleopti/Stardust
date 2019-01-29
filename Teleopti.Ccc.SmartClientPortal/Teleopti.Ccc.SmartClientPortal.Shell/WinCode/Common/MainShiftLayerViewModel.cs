using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class MainShiftLayerViewModel : MoveableLayerViewModel
    {
		private readonly IAuthorization _authorization;
		private readonly MainShiftLayer _layer;
	    private readonly IPersonAssignment _assignment;

	    public MainShiftLayerViewModel(IVisualLayer layer, IPerson person, IAuthorization authorization = null)
            : base(layer, person)
		{
			_authorization = authorization ?? PrincipalAuthorization.Current_DONTUSE();
		}

        public MainShiftLayerViewModel(ILayerViewModelObserver observer, MainShiftLayer layer, IPersonAssignment assignment, IEventAggregator eventAggregator, IAuthorization authorization = null)
				: base(observer, layer, assignment, eventAggregator)
        {
	        _layer = layer;
	        _assignment = assignment;
			_authorization = authorization ?? PrincipalAuthorization.Current_DONTUSE();
		}
		
	    public override string LayerDescription => UserTexts.Resources.Activity;
		
		public override int VisualOrderIndex => _assignment.MainActivities().ToList().IndexOf(_layer);

		public override bool IsMovePermitted()
        {
            if (SchedulePart != null)
            {
                return _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
            }
            return true;
        }

	    public override bool CanMoveUp => _assignment != null && _assignment.MainActivities().ToList().IndexOf(_layer) > 0;

		public override bool CanMoveDown => _assignment != null && !isLayerLast();

		private bool isLayerLast()
		{
			var mainLayers = _assignment.MainActivities().ToList();
			return mainLayers.Any() && mainLayers.Last().Equals(_layer);
		}

		protected override void Replace()
		{
			ParentObservingCollection?.ReplaceActivity(this, Layer as ILayer<IActivity>, SchedulePart);
		}
    }
}