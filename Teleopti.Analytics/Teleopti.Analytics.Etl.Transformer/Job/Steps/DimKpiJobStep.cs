using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimKpiJobStep:JobStepBase
    {


        public DimKpiJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_kpi";
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillKpiDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
        }


    }
}
