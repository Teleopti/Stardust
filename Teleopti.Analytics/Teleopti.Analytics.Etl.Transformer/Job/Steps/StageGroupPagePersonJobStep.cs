using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageGroupPagePersonJobStep : JobStepBase
    {
        public StageGroupPagePersonJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_group_page_person";
            Transformer = new GroupPagePersonTransformer(()=>_jobParameters.StateHolder);
            GroupPagePersonInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        public IGroupPagePersonTransformer Transformer { get; set; }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            IList<IGroupPage> builtInGroupPages = Transformer.BuiltInGroupPages;
            IEnumerable<IGroupPage> userDefinedGroupings = Transformer.UserDefinedGroupings;
            Transformer.Transform(builtInGroupPages, userDefinedGroupings, BulkInsertDataTable1);

            return _jobParameters.Helper.Repository.PersistGroupPagePerson(BulkInsertDataTable1);
        }
    }
}
