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
    /// Created date: 2008-03-11
    /// </remarks>
    public class WeekOfMonth : VolumeYear
    {
        private readonly IWeekOfMonthCreator _weekOfMonthCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeekOfMonth"/> class.
        /// </summary>
        /// <param name="workloadDays">The workload days.</param>
        /// <param name="weekOfMonthCreator">The week of month creator to split into correct parts.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-11
        /// </remarks>
        public WeekOfMonth(ITaskOwnerPeriod workloadDays, IWeekOfMonthCreator weekOfMonthCreator) : base(workloadDays)
        {
            _weekOfMonthCreator = weekOfMonthCreator;
            SetTaskOwnerDaysCollection(workloadDays);
            CreateComparisonPeriod();
            _weekOfMonthCreator.Create(this);
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
            _weekOfMonthCreator.Create(this);
        }

        /// <summary>
        /// Gets the task index for a local date
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public override double TaskIndex(DateOnly dateTime)
        {
            int weekNumber = WeekOfMonthItem.WeekIndex(CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(dateTime.Date));

            return PeriodTypeCollection[weekNumber].TaskIndex;
        }

        /// <summary>
        /// Gets the index of the Talktime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>

        public override double TaskTimeIndex(DateOnly dateTime)
        {
            int weekNumber = WeekOfMonthItem.WeekIndex(CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(dateTime.Date));

            return PeriodTypeCollection[weekNumber].TalkTimeIndex;
        }

        /// <summary>
        /// Gets the index of the aftertalktime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-03-31
        /// </remarks>
        public override double AfterTaskTimeIndex(DateOnly dateTime)
        {
            int weekNumber = WeekOfMonthItem.WeekIndex(CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(dateTime.Date));

            return PeriodTypeCollection[weekNumber].AfterTalkTimeIndex;
        }
    }

    public interface IWeekOfMonthCreator
    {
        void Create(IVolumeYear weekOfMonth);
    }

    public class WeekOfMonthCreator : IWeekOfMonthCreator
    {
        public void Create(IVolumeYear weekOfMonth)
        {
            TaskOwnerHelper taskOwnerHelper = new TaskOwnerHelper(weekOfMonth.TaskOwnerDays);
            IList<TaskOwnerPeriod> list = taskOwnerHelper.CreateWeekTaskOwnerPeriods(CultureInfo.CurrentCulture.Calendar);

            foreach (TaskOwnerPeriod period in list)
            {
                int weekNumber = WeekOfMonthItem.WeekIndex(CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(period.CurrentDate.Date));
                weekOfMonth.PeriodTypeCollection.Add(weekNumber, new WeekOfMonthItem(period, weekOfMonth));
            }
        }
    }
}
