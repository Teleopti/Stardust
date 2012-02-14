using System;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;

namespace Teleopti.Ccc.AgentPortal.AgentStudentAvailabilityView.StudentAvailabilityCellPainters
{
    public class PainterHelper
    {
        private readonly StudentAvailabilityCellData _cellValue;

        public PainterHelper(StudentAvailabilityCellData cellValue)
        {
            _cellValue = cellValue;
        }

        public bool CanPaintDisabled()
        {
            return (!_cellValue.Enabled);
        }

        public bool CanPaintEffectiveRestriction(bool isRightToLeft)
        {
            return (!_cellValue.HasDayOff && !_cellValue.HasAbsence && _cellValue.EffectiveRestriction != null &&
                    !isRightToLeft);
        }

        public bool CanPaintEffectiveRestrictionRightToLeft(bool isRightToLeft)
        {
            return (!_cellValue.HasDayOff && !_cellValue.HasAbsence && _cellValue.EffectiveRestriction != null &&
                    isRightToLeft);
        }

        public bool CanPaintScheduledDayOff()
        {
            return (_cellValue.HasDayOff);
        }

        public bool CanPaintScheduledShift()
        {
            return (_cellValue.HasShift && !_cellValue.HasDayOff);
        }

        public bool HasSchedule()
        {
            return _cellValue.HasShift || _cellValue.HasAbsence || _cellValue.HasDayOff;
        }

        public bool CanPaintAbsence()
        {
            return (_cellValue.HasAbsence && !_cellValue.HasDayOff);
        }

        public bool CanPaintStudentAvailabilityRestrictions()
        {
            return !HasSchedule();
        }

        public bool CanPaintPersonalAssignment()
        {
            return _cellValue.HasPersonalAssignmentOnly;
        }
    }
}