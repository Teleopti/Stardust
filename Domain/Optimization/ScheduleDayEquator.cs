using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IScheduleDayEquator
    {
        bool MainShiftEquals(IScheduleDay original, IScheduleDay current);
        bool DayOffEquals(IScheduleDay original, IScheduleDay current);
    }

    public class ScheduleDayEquator : IScheduleDayEquator
    {
        public bool MainShiftEquals(IScheduleDay original, IScheduleDay current)
        {
            if (!checkMainShiftEqual(original, current))
                return false;
            return true;
        }

        public bool DayOffEquals(IScheduleDay original, IScheduleDay current)
        {
            return checkDayOffEqual(original, current);
        }

        private static bool checkMainShiftEqual(IScheduleDay original, IScheduleDay current)
        {
            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count == 0)
                return true;

            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count > 0)
                return true;

            if (original.SignificantPart() == SchedulePartView.MainShift && current.SignificantPart() == SchedulePartView.DayOff)
                return true;

            if (original.SignificantPart() == SchedulePartView.None && current.SignificantPart() == SchedulePartView.MainShift)
                return false;

            for (int assignmentIndex = 0; assignmentIndex < original.PersonAssignmentCollection().Count; assignmentIndex++)
            {
                IMainShift originalMainShift = original.PersonAssignmentCollection()[assignmentIndex].MainShift;
                IMainShift currentMainShift = current.PersonAssignmentCollection()[assignmentIndex].MainShift;

                if (!mainShiftEquals(originalMainShift, currentMainShift))
                    return false;
            }
            return true;
        }

        private static bool mainShiftEquals(IMainShift original, IMainShift current)
        {
            if(original.ShiftCategory.Id != current.ShiftCategory.Id)
                return false;
            if (original.LayerCollection.Count != current.LayerCollection.Count)
                return false;
            for (int layerIndex = 0; layerIndex < original.LayerCollection.Count; layerIndex++)
            {
                ILayer<IActivity> originalLayer = original.LayerCollection[layerIndex];
                ILayer<IActivity> currentLayer = current.LayerCollection[layerIndex];
                if (!activityEquals(originalLayer, currentLayer))
                    return false;
            }
            return true;
        }

        private static bool activityEquals(ILayer<IActivity> original, ILayer<IActivity> current)
        {
            return original.Period.Equals(current.Period);
        }

        private static bool checkDayOffEqual(IScheduleDay original, IScheduleDay current)
        {
            if (original.SignificantPart() == SchedulePartView.DayOff && current.SignificantPart() == SchedulePartView.DayOff)
                return true;

            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count > 0)
                return true;

            if (original.PersonDayOffCollection().Count == 0 && current.PersonDayOffCollection().Count == 0)
                return true;

            if (original.PersonDayOffCollection().Count > 0 && current.PersonDayOffCollection().Count == 0)
                return true;

            return false;
        }

    }
}
