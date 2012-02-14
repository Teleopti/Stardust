using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class WorkShiftFactory
    {

        public static WorkShift Create(TimeSpan start, TimeSpan end)
        {
            WorkShift retObj = new WorkShift(new ShiftCategory("for test"));
            retObj.LayerCollection.Add(
                new WorkShiftActivityLayer(new Activity("TestActivity"), 
                                           DateTimePeriodForWorkShift(start, end)));
            return retObj;
        }

        public static WorkShift CreateWorkShift(TimeSpan start, TimeSpan end, IActivity activity)
        {
            WorkShift retObj = new WorkShift(new ShiftCategory("for test"));
            retObj.LayerCollection.Add(
                new WorkShiftActivityLayer(activity,
                                           DateTimePeriodForWorkShift(start, end)));
            return retObj;
        }

        public static WorkShift CreateWorkShift(TimeSpan start, TimeSpan end, IActivity activity, IShiftCategory category)
        {
            WorkShift retObj = new WorkShift(category);
            retObj.LayerCollection.Add(
                new WorkShiftActivityLayer(activity,
                                           DateTimePeriodForWorkShift(start, end)));
            return retObj;
        }

        public static WorkShift CreateWorkShiftWithId(TimeSpan start, TimeSpan end, IActivity activity)
        {
            WorkShift retObj = new WorkShift(new ShiftCategory("for test"));
            ((IEntity)retObj).SetId(Guid.NewGuid());
            retObj.LayerCollection.Add(
                new WorkShiftActivityLayer(activity,
                                           DateTimePeriodForWorkShift(start, end)));
            return retObj;
        }

        public static DateTimePeriod DateTimePeriodForWorkShift(TimeSpan start, TimeSpan end)
        {
            return new DateTimePeriod(WorkShift.BaseDate.Add(start),
                                      WorkShift.BaseDate.Add(end));
        }

        public static IWorkShift CreateWithLunch(TimePeriod fullPeriod, TimePeriod lunchPeriod)
        {
            WorkShift retObj = new WorkShift(new ShiftCategory("for test"));
            IActivity activity = new Activity("Test activity");
            IActivity lunch = new Activity("lunch");
            lunch.InContractTime = false;
            retObj.LayerCollection.Add(new WorkShiftActivityLayer(activity, DateTimePeriodForWorkShift(fullPeriod.StartTime, lunchPeriod.StartTime)));
            retObj.LayerCollection.Add(new WorkShiftActivityLayer(lunch, DateTimePeriodForWorkShift(lunchPeriod.StartTime, lunchPeriod.EndTime)));
            retObj.LayerCollection.Add(new WorkShiftActivityLayer(activity, DateTimePeriodForWorkShift(lunchPeriod.EndTime, fullPeriod.EndTime)));

            return retObj;

        }
    }
}
