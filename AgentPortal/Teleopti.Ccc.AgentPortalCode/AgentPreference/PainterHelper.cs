namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public class PainterHelper
    {
        private readonly PreferenceCellData _cellData;

        public PainterHelper(PreferenceCellData cellData)
        {
            _cellData = cellData;
        }
        public bool CanPaintActivityPreference()
        {
            if (_cellData.Preference != null)
                return _cellData.Preference.Activity != null;
            return false;
        }
        
        public bool CanPaintMustHave()
        {
            if (_cellData.Preference != null && _cellData.Preference.MustHave)
                return true;
            return false;
        }

        public bool CanPaintAbsence()
        {
            return (_cellData.HasAbsence && !_cellData.HasDayOff);
        }

        public bool CanPaintDisabled()
        {
            return (!_cellData.Enabled);
        }

        public bool CanPaintEffectiveRestriction(bool rightToLeft)
        {
            if (_cellData.Preference != null && _cellData.Preference.Absence != null)
                return false;

            if (_cellData.ViolatesNightlyRest) return false;

            return (!_cellData.HasDayOff && !_cellData.HasAbsence && _cellData.EffectiveRestriction != null &&
                    !(_cellData.Preference != null && _cellData.Preference.DayOff != null && !_cellData.HasShift) &&
                    !rightToLeft);
        }

        public bool CanPaintViolatesNightlyRest()
        {
            if (!_cellData.HasDayOff && !_cellData.HasAbsence && _cellData.EffectiveRestriction == null)
                return false;

            return _cellData.ViolatesNightlyRest;
        }

        public bool CanPaintEffectiveRestrictionRightToLeft(bool rightToLeft)
        {
            if (_cellData.Preference != null && _cellData.Preference.Absence != null)
                return false;

            return (!_cellData.HasDayOff && !_cellData.HasAbsence && _cellData.EffectiveRestriction != null &&
                    !(_cellData.Preference != null && _cellData.Preference.DayOff != null) &&
                    rightToLeft);
        }

        public bool CanPaintPersonalAssignment()
        {
            if (_cellData.HasPersonalAssignmentOnly)
                return true;
            return false;
        }

        public bool CanPaintPreferredDayOff()
        {
            return (!HasSchedule() && _cellData.Preference != null &&
                    _cellData.Preference.DayOff != null);
        }

        public bool CanPaintPreferredAbsence()
        {
            return (!HasSchedule() && _cellData.Preference != null &&
                    _cellData.Preference.Absence != null);
        }

        public bool CanPaintPreferredExtended()
        {
            return (!HasSchedule() && HasExtendedPreference());
        }

        public bool CanPaintPreferredShiftCategory()
        {
            return (!HasSchedule() && _cellData.Preference != null &&
                    (_cellData.Preference.ShiftCategory != null && _cellData.Preference.Activity == null));
        }

        public bool CanPaintScheduledDayOff()
        {
            return (_cellData.HasDayOff);
        }

        public bool CanPaintScheduledShift()
        {
            return (_cellData.HasShift && !_cellData.HasDayOff);
        }

        public bool HasSchedule()
        {
            return _cellData.HasShift || _cellData.HasAbsence || _cellData.HasDayOff;
        }

        public bool HasExtendedPreference()
        {
            return (_cellData.Preference != null && _cellData.Preference.DayOff == null) && 
                ((_cellData.Preference.StartTimeLimitation.HasValue || _cellData.Preference.EndTimeLimitation.HasValue || _cellData.Preference.WorkTimeLimitation.HasValue) ||
                _cellData.Preference.Activity != null);
        }


        public bool CanPaintExtendedPreferenceTemplate()
        {
            return _cellData.Preference != null && !string.IsNullOrEmpty(_cellData.Preference.TemplateName);
        }
    }
}