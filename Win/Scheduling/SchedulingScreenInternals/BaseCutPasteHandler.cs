using System;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
    public abstract class BaseCutPasteHandler : ICutPasteHandler
    {
        public virtual void Copy()
        {
        }

        public virtual void Paste()
        {
        }

        public virtual void CopySpecial()
        {
        }

        public virtual void PasteSpecial()
        {
        }

        public virtual void Delete()
        {
        }

        public virtual void DeleteSpecial()
        {
        }

        public virtual void Cut()
        {
        }

        public virtual void CutSpecial()
        {
        }

        public virtual void CutAssignment()
        {
        }

        public virtual void CutAbsence()
        {
        }

        public virtual void CutDayOff()
        {
        }

        public virtual void CutPersonalShift()
        {
        }

        public virtual void PasteAssignment()
        {
        }

        public virtual void PasteAbsence()
        {
        }

        public virtual void PasteDayOff()
        {
        }

        public virtual void PastePersonalShift()
        {
        }

        public virtual void PasteShiftFromShifts()
        {
        }
    }
}