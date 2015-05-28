using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class FactAgentStateJobStep : JobStepBase
	{
		public FactAgentStateJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "fact_agent_state";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return _jobParameters.Helper.Repository.AggregateFactAgentStateDataMart(RaptorTransformerHelper.CurrentBusinessUnit);

		}
	}
}