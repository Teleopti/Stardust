using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class BridgeSkillSetSkillJobStep : JobStepBase
	{
		public BridgeSkillSetSkillJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "bridge_skillset_skill";
		}


		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return _jobParameters.Helper.Repository.FillBridgeAgentSkillSetDataMart(RaptorTransformerHelper.CurrentBusinessUnit);

		}
	}
}