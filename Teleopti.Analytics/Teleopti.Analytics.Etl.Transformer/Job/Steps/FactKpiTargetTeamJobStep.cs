using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactKpiTargetTeamJobStep : JobStepBase
    {


        public FactKpiTargetTeamJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "fact_kpi_targets_team";
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillKpiTargetTeamDataMart();
        }


    }
}
