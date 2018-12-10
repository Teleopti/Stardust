using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class FactScheduleDeviationJobStep : JobStepBase
	{
		private readonly bool _isIntraday;

		public FactScheduleDeviationJobStep(IJobParameters jobParameters, bool isIntraday = false)
			: base(jobParameters)
		{
			_isIntraday = isIntraday;

			Name = "fact_schedule_deviation";
			JobCategory = JobCategoryType.AgentStatistics;

			if (jobParameters.NowForTestPurpose.Equals(null))
			{
				DateTime nowForTestPurpose;
				if (DateTime.TryParse(ConfigurationManager.AppSettings["NowForTestPurpose"], out nowForTestPurpose))
					JobParameters.NowForTestPurpose = nowForTestPurpose;
			}
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			const int chunkTimeSpan = 30;
			var affectedRows = 0;

			//assume nightly
			var toDate = JobCategoryDatePeriod.EndDateUtc;
			var fromDate = JobCategoryDatePeriod.StartDateUtc;

			//call once for intraday, dates does not matter for 1st run
			if (_isIntraday)
			{
				var paramIsIntraday = 3;

				affectedRows =
					 _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(new DateTimePeriod(fromDate, toDate),
						  RaptorTransformerHelper.CurrentBusinessUnit,
						  _jobParameters.DefaultTimeZone,
						  paramIsIntraday,
						  _jobParameters.NowForTestPurpose);


				affectedRows +=
					 _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(new DateTimePeriod(fromDate, toDate),
						  RaptorTransformerHelper.CurrentBusinessUnit,
						  _jobParameters.DefaultTimeZone,
						  2,
						  _jobParameters.NowForTestPurpose);

			}
			else
			{
				//call in "nightly"  mode and chunk dates used in Agent period

				for (DateTime startDateTime = fromDate;
					 startDateTime <= toDate;
					 startDateTime = startDateTime.AddDays(chunkTimeSpan))
				{

					DateTime endDateTime = startDateTime.AddDays(chunkTimeSpan) > toDate
						 ? toDate
						 : startDateTime.AddDays(chunkTimeSpan).AddMilliseconds(-1);

					var period = new DateTimePeriod(startDateTime, endDateTime);

					affectedRows += _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(period,
						 RaptorTransformerHelper.CurrentBusinessUnit,
						 _jobParameters.DefaultTimeZone,
						 0,
						 _jobParameters.NowForTestPurpose);
					Result.RowsAffected = affectedRows;
				}
			}
			return affectedRows;
		}
	}
}
