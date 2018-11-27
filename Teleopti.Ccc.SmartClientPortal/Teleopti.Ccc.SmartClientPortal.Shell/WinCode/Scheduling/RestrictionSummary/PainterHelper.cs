namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.RestrictionSummary
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

        public bool CanPaintDisabled()
        {
            return (!_cellValue.Enabled);
        }

        public bool CanPaintEffectiveRestriction(bool rightToLeft)
        {
            return ((_cellValue.EffectiveRestriction != null) && !(_cellValue.HasShift || _cellValue.HasDayOff || _cellValue.HasFullDayAbsence) && !rightToLeft);

        }

        public bool CanPaintEffectiveRestrictionRightToLeft(bool rightToLeft)
        {
            return ((_cellValue.EffectiveRestriction != null) && !(_cellValue.HasShift || _cellValue.HasDayOff || _cellValue.HasFullDayAbsence) && rightToLeft);

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
    }
}