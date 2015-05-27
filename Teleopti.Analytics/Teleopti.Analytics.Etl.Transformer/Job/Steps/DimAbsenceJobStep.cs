using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimAbsenceJobStep : JobStepBase
    {
        public DimAbsenceJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_absence";
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {

            return _jobParameters.Helper.Repository.FillAbsenceDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}