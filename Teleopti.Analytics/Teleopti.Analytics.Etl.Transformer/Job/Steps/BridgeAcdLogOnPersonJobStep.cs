using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class BridgeAcdLogOnPersonJobStep : JobStepBase
    {
        public BridgeAcdLogOnPersonJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "bridge_acd_login_person";
            JobCategory = JobCategoryType.AgentStatistics;
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillBridgeAcdLogOnPerson(RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}