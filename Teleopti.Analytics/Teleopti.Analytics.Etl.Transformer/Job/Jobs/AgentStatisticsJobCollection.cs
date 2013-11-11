using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
    public class AgentStatisticsJobCollection : List<IJobStep>
    {
        public AgentStatisticsJobCollection(IJobParameters jobParameters)
        {
            Add(new StageDateJobStep(jobParameters));
            Add(new StageBusinessUnitJobStep(jobParameters));
            Add(new StagePersonJobStep(jobParameters));
            Add(new StageAgentSkillJobStep(jobParameters));
            Add(new StageScorecardJobStep(jobParameters));
            Add(new DimDateJobStep(jobParameters));
            Add(new DimBusinessUnitJobStep(jobParameters));
            Add(new DimScorecardJobStep(jobParameters));
            Add(new DimSiteJobStep(jobParameters));
            Add(new DimTeamJobStep(jobParameters));
            Add(new DimSkillSetJobStep(jobParameters));
            Add(new DimPersonJobStep(jobParameters));
            Add(new DimQueueJobStep(jobParameters));
            Add(new DimAcdLogOnJobStep(jobParameters));
			Add(new DimQualityQuestLoadJobStep(jobParameters));
            Add(new BridgeAcdLogOnPersonJobStep(jobParameters));
            Add(new FactAgentJobStep(jobParameters));
			Add(new FactScheduleDeviationJobStep(jobParameters));
            Add(new FactAgentQueueJobStep(jobParameters));
            Add(new FactQualityLoadJobStep(jobParameters));
			Add(new FactAgentStateJobStep(jobParameters));
        }
    }
}
