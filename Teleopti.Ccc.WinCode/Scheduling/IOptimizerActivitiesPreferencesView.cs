using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IOptimizerActivitiesPreferencesView
    {
        void KeepShiftCategory(bool keep);
        void KeepStartTime(bool keep);
        void KeepEndTime(bool keep);
        void KeepBetween(TimePeriod? dateTimePeriod);
        void HideForm();
        void Initialize(int resolution, IList<IActivity> allActivities, IList<IActivity> selectedActivities);
        bool IsCanceled();
        IList<IActivity> DoNotMoveActivities();
    
      
    }
}
