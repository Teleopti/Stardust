using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimGroupPageJobStep : JobStepBase
    {

        public DimGroupPageJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_group_page";
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillGroupPagePersonDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
        }


    }
}
