using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageScorecardKpiJobStep : JobStepBase
    {
        public StageScorecardKpiJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_scorecard_kpi";
            ScorecardKpiInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Get data from Raptor
            IList<IScorecard> rootList = _jobParameters.Helper.Repository.LoadScorecard();

            //Transform data from Raptor to Matrix format
            ScorecardKpiTransformer raptorTransformer = new ScorecardKpiTransformer(DateTime.Now);
            raptorTransformer.Transform(rootList, BulkInsertDataTable1);

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistScorecardKpi(BulkInsertDataTable1);
        }
    }
}