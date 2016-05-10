using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class CleanupJobCollection : List<IJobStep>
	{
		public CleanupJobCollection(IJobParameters jobParameters)
		{
			Add(new DimPersonDeleteJobStep(jobParameters));     // BU independent
			Add(new DimPersonTrimJobStep(jobParameters));     // BU independent
			Add(new DimTimeZoneDeleteJobStep(jobParameters));   // BU independent
			Add(new MaintenanceJobStep(jobParameters));     // BU independent
			Add(new PurgeJobStep(jobParameters));     // BU independent
			Add(new SqlServerUpdateStatistics(jobParameters));
		}
	}
}