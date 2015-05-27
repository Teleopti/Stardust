using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactAgentSkillJobStep : JobStepBase
    {
        public FactAgentSkillJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "fact_agent_skill";
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillFactAgentSkillDataMart(RaptorTransformerHelper.CurrentBusinessUnit);             

        }
    }
}