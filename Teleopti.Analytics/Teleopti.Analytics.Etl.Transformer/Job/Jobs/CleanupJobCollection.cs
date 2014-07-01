using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class CleanupJobCollection : List<IJobStep>
    {
        public CleanupJobCollection(IJobParameters jobParameters)
        {
            Add(new DimPersonDeleteJobStep(jobParameters));     // BU independent
            Add(new DimPersonTrimJobStep(jobParameters));     // BU independent
            Add(new DimScenarioDeleteJobStep(jobParameters));   // BU independent
			Add(new DimTimeZoneDeleteJobStep(jobParameters));   // BU independent
            Add(new MaintenanceJobStep(jobParameters));     // BU independent
            Add(new PurgeJobStep(jobParameters));     // BU independent
			Add(new SqlServerUpdateStatistics(jobParameters));
        }
    }
}