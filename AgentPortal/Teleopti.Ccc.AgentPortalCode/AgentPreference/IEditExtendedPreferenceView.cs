using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    public interface IEditExtendedPreferenceView
    {
        void PopulateShiftCategories(IEnumerable<ShiftCategory> shiftCategories);
        void PopulateDaysOff(IEnumerable<DayOff> daysOff);
        void PopulateActivities(IEnumerable<Activity> activities);
        void PopulateAbsences(IEnumerable<Absence> absences);

        void HideView();

        bool ActivityViewVisible { set; get; }

        bool DayOffEnabled { set; get; }
        bool ShiftCategoryEnabled { set; get; }
        bool AbsenceEnabled { set; get; }
        bool ShiftTimeControlsEnabled { set; get; }
        bool ActivityEnabled { set; get; }
        bool ActivityTimeControlsEnabled { set; get; }
        bool SaveButtonEnabled { set; get; }
        bool EndTimeLimitationMinNextDayEnabled { set; get; }
        bool EndTimeLimitationMaxNextDayEnabled { set; get; }

        ShiftCategory ShiftCategory { set; get; }
        DayOff DayOff { set; get; }
        Absence Absence { set; get; }
        TimeSpan? StartTimeLimitationMin { set; get; }
        TimeSpan? StartTimeLimitationMax { set; get; }
        TimeSpan? EndTimeLimitationMin { set; get; }
        TimeSpan? EndTimeLimitationMax { set; get; }
        bool EndTimeLimitationMinNextDay { set; get; }
        bool EndTimeLimitationMaxNextDay { set; get; }
        TimeSpan? WorkTimeLimitationMin { set; get; }
        TimeSpan? WorkTimeLimitationMax { set; get; }

        Activity Activity { set; get; }
        TimeSpan? ActivityEndTimeLimitationMin { set; get; }
        TimeSpan? ActivityEndTimeLimitationMax { set; get; }
        TimeSpan? ActivityStartTimeLimitationMin { set; get; }
        TimeSpan? ActivityStartTimeLimitationMax { set; get; }
        TimeSpan? ActivityTimeLimitationMin { set; get; }
        TimeSpan? ActivityTimeLimitationMax { set; get; }

        string StartTimeLimitationErrorMessage { set; get; }
        string EndTimeLimitationErrorMessage { set; get; }
        string WorkTimeLimitationErrorMessage { set; get; }
        string ActivityStartTimeLimitationErrorMessage { set; get; }
        string ActivityEndTimeLimitationErrorMessage { set; get; }
        string ActivityTimeLimitationErrorMessage { set; get; }
    }
}