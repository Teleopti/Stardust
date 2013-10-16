using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex
{
    public interface IDayInMonthCreator
    {
        void Create(IVolumeYear dayInMonth);
    }

    public class DayInMonthCreator : IDayInMonthCreator
    {
        public void Create(IVolumeYear dayInMonth)
        {
            //create a list to hold data temporary
            var temps = new List<TempData>();
            for (int i = 0; i < 30; i++)
            {
                temps.Add(new TempData { Day = i + 1 });
            }
            double totalSum = 0;
            var days = dayInMonth.TaskOwnerDays;
            foreach (var taskOwner in days)
            {
                var idx = DayInMonthHelper.DayIndex(taskOwner.CurrentDate);
                var temp = temps[idx - 1];
                temp.Tasks.Add(taskOwner.Tasks);
                totalSum += taskOwner.Tasks;
            }
            double totalAvg = 0;
            if (days.Count > 0)
                totalAvg = totalSum / days.Count;

            foreach (var tempData in temps)
            {
                double index = 1;
                if (totalAvg > 0 && tempData.AvgTasks > 0)
                    index = 1 + (tempData.AvgTasks - totalAvg) / totalAvg;
                dayInMonth.PeriodTypeCollection.Add(tempData.Day, new DayInMonthItem { TaskIndex = index, AfterTalkTimeIndex = 1, TalkTimeIndex = 1});
            }
        }

        internal class TempData
        {
            public TempData()
            {
                Tasks = new List<double>();
            }

            public int Day { get; set; }
            public IList<double> Tasks { get; private set; }

            public double AvgTasks
            {
                get
                {
                    if (Tasks.Count == 0) return 0;
                    return Tasks.Sum() / Tasks.Count;
                }
            }
        }
    }
}