using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
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