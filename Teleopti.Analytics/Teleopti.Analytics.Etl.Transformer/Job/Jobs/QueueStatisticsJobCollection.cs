using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;


namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class QueueStatisticsJobCollection : List<IJobStep>
    {
        public QueueStatisticsJobCollection(IJobParameters jobParameters)
        {
            Add(new StageDateJobStep(jobParameters));
            Add(new DimDateJobStep(jobParameters));
            Add(new DimQueueJobStep(jobParameters));
            Add(new FactQueueJobStep(jobParameters));
        }
    }
}
