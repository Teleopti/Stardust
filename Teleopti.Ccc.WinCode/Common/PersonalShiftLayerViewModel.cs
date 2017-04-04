using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Editor;

namespace Teleopti.Ccc.WinCode.Common
{
	public class PersonalShiftLayerViewModel : LayerViewModel
	{
		private readonly PersonalShiftLayer _layer;
		private readonly IPersonAssignment _parent;

		public PersonalShiftLayerViewModel(ILayerViewModelObserver observer, PersonalShiftLayer layer, IPersonAssignment parent, IEventAggregator eventAggregator)
			: base(observer, layer, eventAggregator, false)
		{
			_layer = layer;
			_parent = parent;
		}

		public override string LayerDescription
		{
			get { return UserTexts.Resources.PersonalShifts; }
		}

		public override int VisualOrderIndex
		{
			get { return 200 + _parent.PersonalActivities().ToList().IndexOf(_layer); }
		}

		public override bool IsMovePermitted()
		{
			if (SchedulePart != null)
			{
				return PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			}
			return true;
		}

		public override bool CanMoveUp
		{
			get
			{
				return _parent != null && _parent.PersonalActivities().ToList().IndexOf(_layer) > 0;
			}
		}

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
			if (ParentObservingCollection != null)
				ParentObservingCollection.ReplaceActivity(this, _layer, SchedulePart);
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