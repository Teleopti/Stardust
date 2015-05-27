using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class PerformanceManagerJobCollection : List<IJobStep>
    {
        public PerformanceManagerJobCollection(IJobParameters jobParameters)
        {
            Add(new PerformanceManagerJobStep(jobParameters));
        }
    }

}