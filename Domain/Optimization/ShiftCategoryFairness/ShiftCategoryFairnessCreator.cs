using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{
    public class ShiftCategoryFairnessCreator
    {
		public IShiftCategoryFairnessHolder CreatePersonShiftCategoryFairness(IScheduleRange scheduleRange, DateOnlyPeriod period)
        {
            Dictionary<IShiftCategory, int> shiftDic = new Dictionary<IShiftCategory, int>();

            IEnumerable<IScheduleDay> scheduleDays = scheduleRange.ScheduledDayCollection(period);
            foreach (IScheduleDay scheduleDay in scheduleDays)
            {
                if (scheduleDay.SignificantPart() != SchedulePartView.MainShift)
                    continue;

                IPersonAssignment assignment = scheduleDay.PersonAssignment();
                IShiftCategory shiftCategory = assignment.ShiftCategory;

                if (!shiftDic.ContainsKey(shiftCategory))
                    shiftDic.Add(shiftCategory, 0);

                shiftDic[shiftCategory]++;

            }

            return new ShiftCategoryFairnessHolder(shiftDic);
        }
    }
}