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
    public class MonthOfYear : VolumeYear
    {
        private readonly IMonthOfYearCreator _monthOfYearCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthOfYear"/> class.
        /// </summary>
        /// <param name="workloadDays">The workload days.</param>
        /// <param name="monthOfYearCreator">The month of year creator which create correct parts for this.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-03-11
        /// </remarks>
        public MonthOfYear(ITaskOwnerPeriod workloadDays, IMonthOfYearCreator monthOfYearCreator) : base(workloadDays)
        {
            _monthOfYearCreator = monthOfYearCreator;
            SetTaskOwnerDaysCollection(workloadDays);
            CreateComparisonPeriod();
            _monthOfYearCreator.Create(this);
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
            _monthOfYearCreator.Create(this);
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
            int monthNumber = CultureInfo.CurrentCulture.Calendar.GetMonth(dateTime.Date);
            return PeriodTypeCollection[monthNumber].TaskIndex;
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
            int monthNumber = CultureInfo.CurrentCulture.Calendar.GetMonth(dateTime.Date);
            return PeriodTypeCollection[monthNumber].TalkTimeIndex;
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
            int monthNumber = CultureInfo.CurrentCulture.Calendar.GetMonth(dateTime.Date);
            return PeriodTypeCollection[monthNumber].AfterTalkTimeIndex;
        }
    }
}
