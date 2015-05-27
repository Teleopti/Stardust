using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;


namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
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

            AgentSkillTransform transformer = new AgentSkillTransform();
            transformer.Transform(rootList, BulkInsertDataTable1);

            int affectedRows = _jobParameters.Helper.Repository.PersistAgentSkill(BulkInsertDataTable1);    

            return affectedRows;
        }
    }
}
