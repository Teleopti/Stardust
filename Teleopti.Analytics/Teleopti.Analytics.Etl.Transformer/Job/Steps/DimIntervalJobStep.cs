using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class DimIntervalJobStep : JobStepBase
    {
        public DimIntervalJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "dim_interval";
            IsBusinessUnitIndependent = true;
            IntervalInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
           
            IList<Interval> intervalList = IntervalFactory.CreateIntervalCollection(_jobParameters.IntervalsPerDay);

            // Get data into datatable and bulk insert it to Matrix
            var raptorTransformer = new IntervalTransformer(DateTime.Now);
            raptorTransformer.Transform(intervalList, BulkInsertDataTable1);

            return _jobParameters.Helper.Repository.PersistInterval(BulkInsertDataTable1);

        }
    }
}