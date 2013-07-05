﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class SchedulePartSignificantPartDefinitions : ISignificantPartProvider
    {
        private readonly IScheduleDay _schedulePart;
        private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;
        private IVisualLayerCollection _layerCollcetion;

        public SchedulePartSignificantPartDefinitions(IScheduleDay schedulePart, IHasContractDayOffDefinition hasContractDayOffDefinition)
        {
            InParameter.NotNull("schedulePart", schedulePart);
            _schedulePart = schedulePart;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
        }

        public IDisposable BeginRead()
        {
            _layerCollcetion = _schedulePart.ProjectionService().CreateProjection();
            return new SignificantPartReadAction(this);
        }

        private void End()
        {
            _layerCollcetion = null;
        }

        public bool HasDayOff()
        {
            return _schedulePart.PersonDayOffCollection().Count > 0;
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
            IVisualLayerCollection layerCollection = _layerCollcetion ?? _schedulePart.ProjectionService().CreateProjection();
            foreach (IVisualLayer layer in layerCollection)
            {
                if (!(layer.Payload is IAbsence))
                    return false;
            }

            return layerCollection.HasLayers ? true : false;
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
            return _schedulePart.PersonAssignmentCollection().Count > 0;
        }

        public bool HasPersonalShift()
        {
            IPersonAssignment assignmentToCheck = _schedulePart.PersonAssignment();
            if (assignmentToCheck != null) return assignmentToCheck.PersonalLayers().Any();
            return false;
        }

        public bool HasOvertimeShift()
        {
            IPersonAssignment assignmentToCheck = _schedulePart.PersonAssignment();
            if(assignmentToCheck != null) return assignmentToCheck.OvertimeLayers().Any();
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
