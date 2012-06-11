using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimQualityQuestLoadJobStep : JobStepBase
    {
        public DimQualityQuestLoadJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_quality_quest";
            JobCategory = JobCategoryType.AgentStatistics;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Load Quality Questions to datamart
            return _jobParameters.Helper.Repository.LoadQualityQuestDataMart(_jobParameters.DataSource,
                                                                             RaptorTransformerHelper.
                                                                                 CurrentBusinessUnit);
        }
    }
}