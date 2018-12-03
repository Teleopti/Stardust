using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


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

        public static DateTimePeriod DateTimePeriodForWorkShift(TimeSpan start, TimeSpan end)
        {
            return new DateTimePeriod(WorkShift.BaseDate.Add(start),
                                      WorkShift.BaseDate.Add(end));
        }

        public static IWorkShift CreateWithLunch(TimePeriod fullPeriod, TimePeriod lunchPeriod)
        {
        	var shiftCategory = new ShiftCategory("for test");
			shiftCategory.SetId(Guid.NewGuid());
			WorkShift retObj = new WorkShift(shiftCategory);
            IActivity activity = new Activity("Test activity");
			activity.SetId(Guid.NewGuid());
            IActivity lunch = new Activity("lunch");
			lunch.SetId(Guid.NewGuid());
            lunch.InContractTime = false;
            retObj.LayerCollection.Add(new WorkShiftActivityLayer(activity, DateTimePeriodForWorkShift(fullPeriod.StartTime, lunchPeriod.StartTime)));
            retObj.LayerCollection.Add(new WorkShiftActivityLayer(lunch, DateTimePeriodForWorkShift(lunchPeriod.StartTime, lunchPeriod.EndTime)));
            retObj.LayerCollection.Add(new WorkShiftActivityLayer(activity, DateTimePeriodForWorkShift(lunchPeriod.EndTime, fullPeriod.EndTime)));

            return retObj;

        }
    }
}
