using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;


namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class WorkloadQueuesJobCollection : List<IJobStep>
    {
        public WorkloadQueuesJobCollection(IJobParameters jobParameters)
        {
            Add(new StageBusinessUnitJobStep(jobParameters));
            Add(new StageSkillJobStep(jobParameters));
            Add(new StageWorkloadJobStep(jobParameters));
            Add(new DimBusinessUnitJobStep(jobParameters));
            Add(new DimSkillJobStep(jobParameters));
            Add(new DimWorkloadJobStep(jobParameters));
            Add(new DimQueueJobStep(jobParameters));
            Add(new BridgeQueueWorkloadJobStep(jobParameters));
        }
    }
}
