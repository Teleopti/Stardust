namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
    public class PainterHelper
    {
        private IPreferenceCellData _cellValue;

        public PainterHelper(IPreferenceCellData cellValue)
        {
            _cellValue = cellValue;
        }
        public bool CanPaintActivityPreference()
        {
            if (_cellValue.HasActivityPreference && _cellValue.SchedulingOption.UsePreferences)
                return true;
            return false;
        }
        
        public bool CanPaintMustHave()
        {
            if (_cellValue.EffectiveRestriction != null && _cellValue.MustHavePreference)
                return true;
            return false;
        }

        public bool CanPaintAbsenceOnContractDayOff()
        {
            return (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasFullDayAbsence && !_cellValue.HasDayOff && _cellValue.HasAbsenceOnContractDayOff);
        }

        public bool CanPaintAbsence()
        {
            return (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasFullDayAbsence && !_cellValue.HasDayOff);
        }

        public bool CanPaintDisabled()
        {
            return (!_cellValue.Enabled);
        }

        public bool CanPaintEffectiveRestriction(bool rightToLeft)
        {
            return ((_cellValue.SchedulingOption.UsePreferences || _cellValue.SchedulingOption.UseRotations ||
                   _cellValue.SchedulingOption.UseAvailability || _cellValue.SchedulingOption.UseStudentAvailability) && (_cellValue.EffectiveRestriction != null) && !(_cellValue.HasShift || _cellValue.HasDayOff || _cellValue.HasFullDayAbsence) && !rightToLeft);

        }

        public bool CanPaintEffectiveRestrictionRightToLeft(bool rightToLeft)
        {
            return ((_cellValue.SchedulingOption.UsePreferences || _cellValue.SchedulingOption.UseRotations ||
                    _cellValue.SchedulingOption.UseAvailability || _cellValue.SchedulingOption.UseStudentAvailability) && (_cellValue.EffectiveRestriction != null) && !(_cellValue.HasShift || _cellValue.HasDayOff || _cellValue.HasFullDayAbsence) && rightToLeft);

        }

        public bool CanPaintPreferredDayOff()
        {
            bool scheduleShouldOverwrite = (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasFullDayAbsence) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasShift) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasDayOff);
            if (scheduleShouldOverwrite)
                return false;
            if ((_cellValue.SchedulingOption.UsePreferences && _cellValue.SchedulingOption.UseRotations) && _cellValue.EffectiveRestriction != null
                && (_cellValue.EffectiveRestriction.DayOffTemplate != null && _cellValue.EffectiveRestriction.ShiftCategory != null))
                return false;
            return ((_cellValue.SchedulingOption.UsePreferences || _cellValue.SchedulingOption.UseRotations ||
                    _cellValue.SchedulingOption.UseAvailability || _cellValue.SchedulingOption.UseStudentAvailability) && (_cellValue.EffectiveRestriction != null && _cellValue.EffectiveRestriction.DayOffTemplate != null));


        }

        public bool CanPaintPreferredExtended()
        {
            bool canPaint = _cellValue.SchedulingOption.UsePreferences;
            if (!canPaint)
                return false;
            return (((!_cellValue.HasDayOff && !_cellValue.HasShift && !_cellValue.HasAbsence && _cellValue.SchedulingOption.UseScheduling) || (!_cellValue.SchedulingOption.UseScheduling)) && _cellValue.HasExtendedPreference);
        }

        public bool CanPaintPreferredShiftCategory()
        {
            bool scheduleShouldOverwrite = (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasFullDayAbsence) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasShift) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasDayOff);
            if (scheduleShouldOverwrite)
                return false;
            if ((_cellValue.SchedulingOption.UsePreferences && _cellValue.SchedulingOption.UseRotations) && _cellValue.EffectiveRestriction != null
                && (_cellValue.EffectiveRestriction.DayOffTemplate != null && _cellValue.EffectiveRestriction.ShiftCategory != null))
                return false;
            return ((_cellValue.SchedulingOption.UsePreferences || _cellValue.SchedulingOption.UseRotations ||
                    _cellValue.SchedulingOption.UseAvailability || _cellValue.SchedulingOption.UseStudentAvailability) && (_cellValue.EffectiveRestriction != null && _cellValue.EffectiveRestriction.ShiftCategory != null));

        }

        public bool CanPaintPreferredAbsence()
        {
            bool scheduleShouldOverwrite = (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasFullDayAbsence) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasShift) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasDayOff);
            if (scheduleShouldOverwrite)
                return false;
            if (_cellValue.HasAbsenceOnContractDayOff)
                return false;
            if ((_cellValue.SchedulingOption.UsePreferences && _cellValue.SchedulingOption.UseRotations) && _cellValue.EffectiveRestriction != null
                && (_cellValue.EffectiveRestriction.DayOffTemplate != null && _cellValue.EffectiveRestriction.Absence != null))
                return false;
            return ((_cellValue.SchedulingOption.UsePreferences || _cellValue.SchedulingOption.UseRotations ||
                    _cellValue.SchedulingOption.UseAvailability || _cellValue.SchedulingOption.UseStudentAvailability) && (_cellValue.EffectiveRestriction != null && _cellValue.EffectiveRestriction.Absence != null));

        }

        public bool CanPaintPreferredAbsenceOnContractDayOff()
        {
            bool scheduleShouldOverwrite = (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasFullDayAbsence) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasShift) || (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasDayOff);
            if (scheduleShouldOverwrite)
                return false;
            if (!_cellValue.HasAbsenceOnContractDayOff)
                return false;
            if ((_cellValue.SchedulingOption.UsePreferences && _cellValue.SchedulingOption.UseRotations) && _cellValue.EffectiveRestriction != null
                && (_cellValue.EffectiveRestriction.DayOffTemplate != null && _cellValue.EffectiveRestriction.Absence != null))
                return false;
            return ((_cellValue.SchedulingOption.UsePreferences || _cellValue.SchedulingOption.UseRotations ||
                    _cellValue.SchedulingOption.UseAvailability || _cellValue.SchedulingOption.UseStudentAvailability) && (_cellValue.EffectiveRestriction != null && _cellValue.EffectiveRestriction.Absence != null));

        }

        public bool CanPaintScheduledDayOff()
        {
            return (_cellValue.SchedulingOption.UseScheduling && _cellValue.HasDayOff);
        }

        public bool CanPaintScheduledShift()
        {
            if (_cellValue.SchedulingOption.UseRotations && _cellValue.HasFullDayAbsence)
                return false;
            return (_cellValue.SchedulingOption.UseScheduling && !_cellValue.HasFullDayAbsence && !_cellValue.HasDayOff && _cellValue.HasShift);

        }
    }
}