using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimScenarioDeleteJobStep : JobStepBase
    {
        public DimScenarioDeleteJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_scenario delete data";
            IsBusinessUnitIndependent = true;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.DimScenarioDeleteData(RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}