using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class SchedulePartSignificantPartDefinitions : ISignificantPartProvider
    {
        private readonly IScheduleDay _schedulePart;
        private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;
        private Lazy<IVisualLayerCollection> _layerCollection;

        public SchedulePartSignificantPartDefinitions(IScheduleDay schedulePart, IHasContractDayOffDefinition hasContractDayOffDefinition)
        {
            InParameter.NotNull(nameof(schedulePart), schedulePart);
            _schedulePart = schedulePart;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;

			_layerCollection = new Lazy<IVisualLayerCollection>(() => _schedulePart.ProjectionService().CreateProjection());
        }

        public IDisposable BeginRead()
        {
            return new SignificantPartReadAction(this);
        }

        private void End()
        {
			_layerCollection = new Lazy<IVisualLayerCollection>(() => _schedulePart.ProjectionService().CreateProjection());
        }

        public bool HasDayOff()
        {
            return _schedulePart.HasDayOff();
        }

        public virtual bool HasContractDayOff()
        {
            if (!HasFullAbsence())
				return false;

			if (HasMainShift())
				return false;

			if (HasDayOff())
				return false;

			return _hasContractDayOffDefinition.IsDayOff(_schedulePart);
        }

        public bool HasFullAbsence()
        {
            IVisualLayerCollection layerCollection = _layerCollection.Value;
            foreach (IVisualLayer layer in layerCollection)
            {
                if (!(layer.Payload is IAbsence))
                    return false;
            }

            return layerCollection.HasLayers;
        }

        public bool HasAbsence()
        {
            return !_schedulePart.PersonAbsenceCollection().IsEmpty();
        }

        public bool HasMainShift()
        {
            IPersonAssignment assignmentToCheck = _schedulePart.PersonAssignment();
            if (assignmentToCheck != null) return assignmentToCheck.ShiftCategory != null;
            return false;
        }


        public bool HasAssignment()
        {
            return _schedulePart.PersonAssignment() != null;
        }

        public bool HasPersonalShift()
        {
            IPersonAssignment assignmentToCheck = _schedulePart.PersonAssignment();
            if (assignmentToCheck != null) return assignmentToCheck.PersonalActivities().Any();
            return false;
        }

        public bool HasOvertimeShift()
        {
            IPersonAssignment assignmentToCheck = _schedulePart.PersonAssignment();
            if(assignmentToCheck != null) return assignmentToCheck.OvertimeActivities().Any();
            return false;
        }

        public bool HasPreferenceRestriction()
        {
            return !_schedulePart.RestrictionCollection().FilterBySpecification(RestrictionMustBe.Preference).IsEmpty();
        }

        public bool HasStudentAvailabilityRestriction()
        {
            return !_schedulePart.RestrictionCollection().FilterBySpecification(RestrictionMustBe.StudentAvailability).IsEmpty();
        }

        private class SignificantPartReadAction : IDisposable
        {
            private readonly SchedulePartSignificantPartDefinitions _schedulePartSignificantPartDefinitions;

            public SignificantPartReadAction(SchedulePartSignificantPartDefinitions schedulePartSignificantPartDefinitions)
            {
                _schedulePartSignificantPartDefinitions = schedulePartSignificantPartDefinitions;
            }

            public void Dispose()
            {
                _schedulePartSignificantPartDefinitions.End();
            }
        }
    }
}
