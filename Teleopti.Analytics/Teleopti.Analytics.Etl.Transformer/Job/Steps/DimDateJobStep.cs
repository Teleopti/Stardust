using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimDateJobStep : JobStepBase
    {
        public DimDateJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_date";
            IsBusinessUnitIndependent = true;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
           

            //Load data from stage to datamart

            return _jobParameters.Helper.Repository.FillDateDataMart();


        }
    }
}