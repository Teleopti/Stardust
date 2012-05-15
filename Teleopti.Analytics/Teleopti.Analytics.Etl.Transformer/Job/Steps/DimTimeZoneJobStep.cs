using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimTimeZoneJobStep : JobStepBase
    {
        public DimTimeZoneJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_time_zone";
            IsBusinessUnitIndependent = true;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
         
            return _jobParameters.Helper.Repository.FillTimeZoneDimDataMart(RaptorTransformerHelper.CurrentBusinessUnit);

        }
    }
}