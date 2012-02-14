using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{

    public interface IShiftCategoryFairnessCreator
    {
        IShiftCategoryFairness CreatePersonShiftCategoryFairness(IScheduleRange scheduleRange, DateOnlyPeriod period);
    }

    public class ShiftCategoryFairnessCreator : IShiftCategoryFairnessCreator
    {

        public IShiftCategoryFairness CreatePersonShiftCategoryFairness(IScheduleRange scheduleRange, DateOnlyPeriod period)
        {
            Dictionary<IShiftCategory, int> shiftDic = new Dictionary<IShiftCategory, int>();

            IEnumerable<IScheduleDay> scheduleDays = scheduleRange.ScheduledDayCollection(period);
            foreach (IScheduleDay scheduleDay in scheduleDays)
            {
                if (scheduleDay.SignificantPart() != SchedulePartView.MainShift)
                    continue;

                IPersonAssignment assignment = scheduleDay.AssignmentHighZOrder();
                IShiftCategory shiftCategory = assignment.MainShift.ShiftCategory;

                if (!shiftDic.ContainsKey(shiftCategory))
                    shiftDic.Add(shiftCategory, 0);

                shiftDic[shiftCategory]++;

            }

        	IFairnessValueResult fairnessValueResult = scheduleRange.FairnessPoints();

            return new ShiftCategoryFairness(shiftDic, fairnessValueResult);
        }
    }
}