using System;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public interface IMonthOfYearCreator
    {
        void Create(IVolumeYear monthOfYear);
    }

    public class MonthOfYearCreator : IMonthOfYearCreator
    {
        public void Create(IVolumeYear monthOfYear)
        {
            TaskOwnerHelper taskOwnerHelper = new TaskOwnerHelper(monthOfYear.TaskOwnerDays);
            IList<TaskOwnerPeriod> list = taskOwnerHelper.CreateMonthTaskOwnerPeriods(CultureInfo.CurrentCulture.Calendar);

            foreach (TaskOwnerPeriod period in list)
            {
                int monthNumber = CultureInfo.CurrentCulture.Calendar.GetMonth(period.CurrentDate.Date);
                monthOfYear.PeriodTypeCollection.Add(monthNumber, new MonthOfYearItem(period, monthOfYear));
            }
        }
    }
}