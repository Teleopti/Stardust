using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-03-12
    /// </remarks>
    public class DayOfWeeks : VolumeYear
    {
        private readonly IDaysOfWeekCreator _daysOfWeekCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfWeek"/> class.
        /// </summary>
        /// <param name="workloadDays">The workload days.</param>
        /// <param name="daysOfWeekCreator">The days of week creator to split workload days into correct parts.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-12
        /// </remarks>
        public DayOfWeeks(ITaskOwnerPeriod workloadDays, IDaysOfWeekCreator daysOfWeekCreator) : base(workloadDays)
        {
            _daysOfWeekCreator = daysOfWeekCreator;
            SetTaskOwnerDaysCollection(workloadDays);

            CreateComparisonPeriod();
            _daysOfWeekCreator.Create(this);
        }

        /// <summary>
        /// Reloads the historical data depth.
        /// </summary>
        /// <param name="workloadDays">The workload days.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-12
        /// </remarks>
        public override void ReloadHistoricalDataDepth(ITaskOwnerPeriod workloadDays)
        {
            SetTaskOwnerDaysCollection(workloadDays);
            ResetPeriodTypeCollection();
            CreateComparisonPeriod();
            _daysOfWeekCreator.Create(this);
        }

        /// <summary>
        /// Gets the task index for a date
        /// </summary>
        /// <param name="dateTime">The date time (local time).</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public override double TaskIndex(DateOnly dateTime)
        {
            int dayNumber = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(dateTime.Date) + 1;


            return PeriodTypeCollection[dayNumber].TaskIndex;
        }

        /// <summary>
        /// Gets the index of the Talktime.
        /// </summary>
        /// <param name="dateTime">The date time (local time).</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>

        public override double TaskTimeIndex(DateOnly dateTime)
        {
            int dayNumber = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(dateTime.Date) + 1;


            return PeriodTypeCollection[dayNumber].TalkTimeIndex;
        }

        /// <summary>
        /// Gets the index of the aftertalktime.
        /// </summary>
        /// <param name="dateTime">The date time (local time).</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public override double AfterTaskTimeIndex(DateOnly dateTime)
        {
            int dayNumber = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(dateTime.Date) + 1;


            return PeriodTypeCollection[dayNumber].AfterTalkTimeIndex;
        }
    }

    public interface IDaysOfWeekCreator
    {
        void Create(IVolumeYear dayOfWeeks);
    }

    public class DaysOfWeekCreator : IDaysOfWeekCreator
    {
        public void Create(IVolumeYear dayOfWeeks)
        {
            TaskOwnerHelper taskOwnerHelper = new TaskOwnerHelper(dayOfWeeks.TaskOwnerDays);
            IList<TaskOwnerPeriod> list = taskOwnerHelper.CreateDayTaskOwnerPeriods(CultureInfo.CurrentCulture.Calendar);

            foreach (TaskOwnerPeriod period in list)
            {
                int dayNumber = (int)CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(period.CurrentDate.Date) + 1;
                dayOfWeeks.PeriodTypeCollection.Add(dayNumber, new DayOfWeekItem(period, dayOfWeeks));
            }
        }
    }
}
