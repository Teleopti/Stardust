using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageSkillJobStep : JobStepBase
    {
        public StageSkillJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_skill";
            SkillInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Get data from Raptor
            IList<ISkill> rootList = _jobParameters.StateHolder.SkillCollection;
            
            //Transform data from Raptor to Matrix format
            SkillTransformer raptorTransformer = new SkillTransformer(DateTime.Now);
            raptorTransformer.Transform(rootList, BulkInsertDataTable1);

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistSkill(BulkInsertDataTable1);

        }
    }
}