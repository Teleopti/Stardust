using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
    public class StageAgentSkillJobStep : JobStepBase
    {
        public StageAgentSkillJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_agent_skill";
            //DataSet MyDataSet;
            //MyDataSet.Tables[PersonSkillInfrastructure.TableName]
            PersonSkillInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            IEnumerable<IPerson> rootList = _jobParameters.StateHolder.PersonCollection;

            var transformer = new AgentSkillTransform();
            transformer.Transform(rootList, BulkInsertDataTable1);

            int affectedRows = _jobParameters.Helper.Repository.PersistAgentSkill(BulkInsertDataTable1);    

            return affectedRows;
        }
    }
}
