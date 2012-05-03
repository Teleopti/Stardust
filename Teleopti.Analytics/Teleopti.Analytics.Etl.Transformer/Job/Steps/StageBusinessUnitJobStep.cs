using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageBusinessUnitJobStep : JobStepBase
    {
        public StageBusinessUnitJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_business_unit";
            IsBusinessUnitIndependent = true;
            BusinessUnitInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Get data from Raptor
            IList<IBusinessUnit> rootList = _jobParameters.Helper.Repository.LoadBusinessUnit();

            //Transform data from Raptor to Matrix format
            BusinessUnitTransformer raptorTransformer = new BusinessUnitTransformer(DateTime.Now);
            raptorTransformer.Transform(rootList, BulkInsertDataTable1);

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistBusinessUnit(BulkInsertDataTable1);

        }
    }
}