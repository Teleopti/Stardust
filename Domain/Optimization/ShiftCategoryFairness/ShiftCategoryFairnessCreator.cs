using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness
{

    public interface IShiftCategoryFairnessCreator
    {
        IShiftCategoryFairnessHolder CreatePersonShiftCategoryFairness(IScheduleRange scheduleRange, DateOnlyPeriod period);
    }

    public class ShiftCategoryFairnessCreator : IShiftCategoryFairnessCreator
    {

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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

        	IFairnessValueResult fairnessValueResult = scheduleRange.FairnessValue();

            return new ShiftCategoryFairnessHolder(shiftDic, fairnessValueResult);
        }
    }
}