using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Editor;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public class PersonalShiftLayerViewModel : LayerViewModel
    {
	    private readonly IPersonalShift _parent;

	    public PersonalShiftLayerViewModel(ILayerViewModelObserver observer, ILayer<IActivity> layer, IPersonalShift parent, IEventAggregator eventAggregator)
            : base(observer,layer, eventAggregator, false)
        {
	        _parent = parent;
        }


	    public override string LayerDescription
        {
            get { return UserTexts.Resources.PersonalShifts; }
        }

        protected override int OrderIndexBase
        {
            get
            {
                var idx = 0;
                if (_parent != null)
                    idx = _parent.OrderIndex;
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

        public override bool CanMoveUp
        {
            get
            {
	            var personalShift = getPersonalShift();
	            var personAssignment = getPersonAssignment(personalShift);
				if (personAssignment != null)
				{
					var index = personAssignment.PersonalShiftCollection.IndexOf(personalShift);
					return personAssignment.PersonalShiftCollection.IndexOf(personalShift) > 0;
				}

	            return false;
            }
        }

        public override bool CanMoveDown
        {
			get
			{
				var personalShift = getPersonalShift();
				var personAssignment = getPersonAssignment(personalShift);
				if (personAssignment != null)
				{
					return personAssignment.PersonalShiftCollection.Contains(personalShift) && personAssignment.PersonalShiftCollection.Last() != personalShift;
				}

				return false;
			}
        }

		public override void MoveDown()
		{
			if (CanMoveDown)
			{
				movePersonalShift(false);
			}

		}

		public override void MoveUp()
		{
			if (CanMoveUp)
			{
				movePersonalShift(true);
			}
		}

        public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
        {
            return false;
        }

		private void movePersonalShift(bool moveUp)
		{
			var personalShift = getPersonalShift();
			var personAssignment = getPersonAssignment(personalShift);
			if (personAssignment != null)
			{
				var index = personAssignment.PersonalShiftCollection.IndexOf(personalShift);
				personAssignment.RemovePersonalShift(personalShift);
				IList<IPersonalShift> personalShiftsList = personAssignment.PersonalShiftCollection.ToList();

				if (moveUp) index--;
				else index++;

				if (index == personalShiftsList.Count)
					personalShiftsList.Add(personalShift);
				else
					personalShiftsList.Insert(index, personalShift);

				personAssignment.ClearPersonalShift();
				foreach (var ps in personalShiftsList)
				{
					personAssignment.AddPersonalShift(ps);
				}

			}

			LayerMoved();	
		}

		private void LayerMoved()
		{
			if (ParentObservingCollection != null)
			{
				ParentObservingCollection.LayerMovedVertically(this);
				new TriggerShiftEditorUpdate().PublishEvent("LayerViewModel", LocalEventAggregator);
			}
		}

		private IPersonalShift getPersonalShift()
		{
			return _parent as Domain.Scheduling.Assignment.PersonalShift;
		}

		private IPersonAssignment getPersonAssignment(IPersonalShift personalShift)
		{
			if (personalShift != null)
			{
				var personAssignment = personalShift.Parent as Domain.Scheduling.Assignment.PersonAssignment;
				return personAssignment;
			}

			return null;
		}
    }
}