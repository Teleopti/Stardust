using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class PurgeJobStep : JobStepBase
    {
        public PurgeJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "Purge Old Application Data";
            IsBusinessUnitIndependent = true;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            // Run maintenance sp
            return _jobParameters.Helper.Repository.PerformPurge();
        }
    }
}