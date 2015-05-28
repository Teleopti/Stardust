using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class ForecastJobCollection : List<IJobStep>
	{
		public ForecastJobCollection(IJobParameters jobParameters)
		{
			Add(new StageBusinessUnitJobStep(jobParameters));
			Add(new StageDateJobStep(jobParameters));
			Add(new StageSkillJobStep(jobParameters));
			Add(new StageWorkloadJobStep(jobParameters));
			Add(new StageForecastWorkloadJobStep(jobParameters));
			Add(new StageScenarioJobStep(jobParameters));
			Add(new DimBusinessUnitJobStep(jobParameters));
			Add(new DimDateJobStep(jobParameters));
			Add(new DimSkillJobStep(jobParameters));
			Add(new DimWorkloadJobStep(jobParameters));
			Add(new DimScenarioJobStep(jobParameters));
			Add(new FactForecastWorkloadJobStep(jobParameters));
		}
	}
}
