using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class PersonalShiftLayerViewModel : LayerViewModel
	{
		private readonly PersonalShiftLayer _layer;
		private readonly IPersonAssignment _parent;
		private readonly IAuthorization _authorization;

		public PersonalShiftLayerViewModel(ILayerViewModelObserver observer, PersonalShiftLayer layer, IPersonAssignment parent, IEventAggregator eventAggregator, IAuthorization authorization = null)
			: base(observer, layer, eventAggregator, false, parent.Person)
		{
			_layer = layer;
			_parent = parent;
			_authorization = authorization ?? PrincipalAuthorization.Current_DONTUSE();
		}

		public override string LayerDescription => UserTexts.Resources.PersonalShifts;

		public override int VisualOrderIndex => 200 + _parent.PersonalActivities().ToList().IndexOf(_layer);

		public override bool IsMovePermitted()
		{
			if (SchedulePart != null)
			{
				return _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			}
			return true;
		}

		public override bool CanMoveUp => _parent != null && _parent.PersonalActivities().ToList().IndexOf(_layer) > 0;

		public override bool CanMoveDown
		{
			get
			{
				if (_parent != null)
				{
					return _parent.PersonalActivities().Contains(_layer) && !_parent.PersonalActivities().ToList().Last().Equals(_layer);
				}

				return false;
			}
		}

		public override void MoveDown()
		{
			if (CanMoveDown)
			{
				_parent.MoveLayerDown(_layer);
				LayerMoved();
			}
		}

		public override void MoveUp()
		{
			if (CanMoveUp)
			{
				_parent.MoveLayerUp(_layer);
				LayerMoved();
			}
		}

		public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
		{
			return false;
		}

		protected override void Replace()
		{
			ParentObservingCollection?.ReplaceActivity(this, _layer, SchedulePart);
		}

		private void LayerMoved()
		{
			if (ParentObservingCollection != null)
			{
				ParentObservingCollection.LayerMovedVertically(this);
				new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
			}
		}
	}
}