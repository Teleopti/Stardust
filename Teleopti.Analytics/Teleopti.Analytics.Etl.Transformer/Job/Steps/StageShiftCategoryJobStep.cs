using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageShiftCategoryJobStep : JobStepBase
    {
        public StageShiftCategoryJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_shift_category";
            ShiftCategoryInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Get data from Raptor
            IList<IShiftCategory> rootList = _jobParameters.Helper.Repository.LoadShiftCategory();

            //Transform data from Raptor to Matrix format
            ShiftCategoryTransformer raptorTransformer = new ShiftCategoryTransformer(DateTime.Now);
            raptorTransformer.Transform(rootList, BulkInsertDataTable1);

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistShiftCategory(BulkInsertDataTable1);
        }
    }
}