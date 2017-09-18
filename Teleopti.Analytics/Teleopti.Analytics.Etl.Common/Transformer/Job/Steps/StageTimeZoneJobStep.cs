using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageTimeZoneJobStep : JobStepBase
	{
		public StageTimeZoneJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_time_zone";
			IsBusinessUnitIndependent = true;
			TimeZoneInfrastructure.AddColumnsToDimensionDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Get list of time zones in use
			IList<TimeZoneDim> rootList = new TimeZoneDimFactory().Create(
				_jobParameters.DefaultTimeZone, 
				_jobParameters.StateHolder.TimeZonesUsedByClient, 
				_jobParameters.StateHolder.TimeZonesUsedByDataSources);

			//Transform time zone info to Matrix format
			var transformer = new TimeZoneTransformer(DateTime.Now);
			transformer.TransformDim(rootList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistTimeZoneDim(BulkInsertDataTable1);
		}
	}
}