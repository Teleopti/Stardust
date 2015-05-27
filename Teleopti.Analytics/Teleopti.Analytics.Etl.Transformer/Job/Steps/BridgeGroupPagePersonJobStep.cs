using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class BridgeGroupPagePersonJobStep : JobStepBase
    {

        public BridgeGroupPagePersonJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "bridge_group_page_person";
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillGroupPagePersonBridgeDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
        }


    }
}
