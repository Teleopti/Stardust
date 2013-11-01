
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;


namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class AgentSkillCollection : List<IJobStep>
    {
        public AgentSkillCollection(IJobParameters jobParameters)
        {
            Add(new StageBusinessUnitJobStep(jobParameters));
            Add(new StagePersonJobStep(jobParameters));
            Add(new StageSkillJobStep(jobParameters));
            Add(new StageAgentSkillJobStep(jobParameters));
            Add(new StageScorecardJobStep(jobParameters));
            Add(new DimSkillJobStep(jobParameters));
            Add(new DimBusinessUnitJobStep(jobParameters));
            Add(new DimScorecardJobStep(jobParameters));
            Add(new DimSiteJobStep(jobParameters));
            Add(new DimTeamJobStep(jobParameters));
            Add(new DimSkillSetJobStep(jobParameters));
            Add(new DimPersonJobStep(jobParameters));                        
            Add(new BridgeSkillSetSkillJobStep(jobParameters));
            Add(new FactAgentSkillJobStep(jobParameters));
        }
    }
}
