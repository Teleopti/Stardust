using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DimScenarioJobStep : JobStepBase
	{
		public DimScenarioJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "dim_scenario";
		}


		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return _jobParameters.Helper.Repository.FillScenarioDataMart(RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}