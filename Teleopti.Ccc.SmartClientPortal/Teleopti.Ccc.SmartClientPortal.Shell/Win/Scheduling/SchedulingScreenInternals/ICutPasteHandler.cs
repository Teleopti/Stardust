namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
    public interface ICutPasteHandler
    {
        void Copy();
        void Paste();
        void CopySpecial();
        void PasteSpecial();
        void Delete();
        void DeleteSpecial();
        void Cut();
        void CutSpecial();
        void CutAssignment();
        void CutAbsence();
        void CutDayOff();
        void CutPersonalShift();
        void PasteAssignment();
        void PasteAbsence();
        void PasteDayOff();
        void PastePersonalShift();
        void PasteShiftFromShifts();
    }
}
