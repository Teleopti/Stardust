using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex
{
    public class DayInMonthItem : IPeriodType
    {
        public double TaskIndex { get; set; }
        public double TalkTimeIndex { get; set; }
        public double AfterTalkTimeIndex { get; set; }
        public double AverageTasks { get; set; }
        public double DailyAverageTasks { get; private set; }
        public TimeSpan AverageTalkTime { get; set; }
        public TimeSpan AverageAfterWorkTime { get; set; }
    }
}