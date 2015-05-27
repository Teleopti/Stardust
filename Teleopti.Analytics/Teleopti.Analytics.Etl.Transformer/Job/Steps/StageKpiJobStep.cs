using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageKpiJobStep:JobStepBase
    {


        public StageKpiJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_kpi";
            KpiInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }


        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Get data from Raptor
            IList<IKeyPerformanceIndicator> rootList = _jobParameters.Helper.Repository.LoadKpi();

            //Transform data from Raptor to Matrix format
            KpiTransformer raptorTransformer = new KpiTransformer(DateTime.Now);
            raptorTransformer.Transform(rootList, BulkInsertDataTable1);

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistKpi(BulkInsertDataTable1);

        }


    }
}
