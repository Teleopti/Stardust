using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class BridgeQueueWorkloadJobStep : JobStepBase
    {
        public BridgeQueueWorkloadJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "bridge_queue_workload";
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillBridgeWorkloadQueue();             

        }
    }
}