using System;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public static class JobMultipleDateFactory
    {
        public static IJobMultipleDate CreateJobMultipleDate()
        {
            IJobMultipleDate jobMultipleDate =
                new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

            jobMultipleDate.Add(new DateTime(2008, 1, 1), new DateTime(2008, 1, 7), JobCategoryType.Schedule);
            jobMultipleDate.Add(new DateTime(2008, 2, 1), new DateTime(2008, 2, 28), JobCategoryType.Forecast);
            jobMultipleDate.Add(new DateTime(2007, 12, 28), new DateTime(2008, 1, 1), JobCategoryType.QueueStatistics);
            jobMultipleDate.Add(new DateTime(2007, 12, 28), new DateTime(2008, 1, 1), JobCategoryType.AgentStatistics);

            return jobMultipleDate;
        }
    }
}
