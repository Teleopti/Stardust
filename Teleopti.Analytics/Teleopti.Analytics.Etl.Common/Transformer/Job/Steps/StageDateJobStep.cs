using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;


namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageDateJobStep : JobStepBase
	{
		private readonly bool _isInitial;

		public StageDateJobStep(IJobParameters jobParameters, bool isInitial = false)
			: base(jobParameters)
		{
			Name = "stg_date";
			JobCategory = JobCategoryType.Initial;
			IsBusinessUnitIndependent = true;
			_isInitial = isInitial;
			DateInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			// Get max date in dim_date (to be able to avoid gaps in the date_id sequence)
			var maxDimDate = _jobParameters.Helper.Repository.GetMaxDateInDimDate(_isInitial); 
			var startDate = _jobParameters.DefaultTimeZone.SafeConvertTimeToUtc(maxDimDate);
			var endDate = startDate;

			var minMaxDatePeriodFromAllJobCategorys = _jobParameters.JobCategoryDates.MinMaxDatesUtc;
			if (startDate < minMaxDatePeriodFromAllJobCategorys.EndDateUtc.AddDays(1))
			{
				startDate = startDate.AddDays(1);
				endDate = minMaxDatePeriodFromAllJobCategorys.EndDateUtc.AddDays(1);
			}

			if (startDate > endDate)
			{
				return 0;
			}

			//Get a created list of dates from RaptorTransformer (Not from Raptor)
			var period = new DateTimePeriod(startDate, endDate);
			var raptorTransformer = new DateTransformer(DateTime.Now);
			var dateList = DateTransformer.CreateDateList(period);
			raptorTransformer.Transform(dateList, BulkInsertDataTable1);

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistDate(BulkInsertDataTable1);
		}
	}
}