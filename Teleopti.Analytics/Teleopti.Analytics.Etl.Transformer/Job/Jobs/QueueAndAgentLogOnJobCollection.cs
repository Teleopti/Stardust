using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;


namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class QueueAndAgentLogOnJobCollection : List<IJobStep>
    {
        public QueueAndAgentLogOnJobCollection(IJobParameters jobParameters)
        {
            Add(new DimQueueJobStep(jobParameters));
            Add(new DimAcdLogOnJobStep(jobParameters));
            Add(new RaptorQueueSynchronizationStep(jobParameters));
            Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));
        }
    }
}
