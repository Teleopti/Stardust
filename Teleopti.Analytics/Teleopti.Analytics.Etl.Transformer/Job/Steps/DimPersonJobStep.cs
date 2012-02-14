using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimPersonJobStep : JobStepBase
    {
        public DimPersonJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_person";
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillPersonDataMart(Result.CurrentBusinessUnit);
        }
    }
}