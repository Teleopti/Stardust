using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

//using JobParameters=RaptorTransformer.Job.JobParameters;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageDateJobStep : JobStepBase
    {
        public StageDateJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_date";
            JobCategory = JobCategoryType.Initial;
            IsBusinessUnitIndependent = true;
            DateInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            // Get max date in dim_date (to be able to avoid gaps in the date_id sequence)
            TimeZoneInfo timeZoneInfo = _jobParameters.DefaultTimeZone;
            DateTime startDate = timeZoneInfo.SafeConvertTimeToUtc(_jobParameters.Helper.Repository.GetMaxDateInDimDate());
            DateTime endDate = startDate;

            IJobMultipleDateItem minMaxDatePeriodFromAllJobCategorys = _jobParameters.JobCategoryDates.MinMaxDatesUtc;
            if (startDate < minMaxDatePeriodFromAllJobCategorys.EndDateUtc.AddDays(1))
            {
                startDate = startDate.AddDays(1);
                endDate = minMaxDatePeriodFromAllJobCategorys.EndDateUtc.AddDays(1);
            }

            //Get a created list of dates from RaptorTransformer (Not from Raptor)
            DateTimePeriod period = new DateTimePeriod(startDate, endDate);
            DateTransformer raptorTransformer = new DateTransformer(DateTime.Now);
            IList<DayDate> dateList = DateTransformer.CreateDateList(period);
            raptorTransformer.Transform(dateList, BulkInsertDataTable1);

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistDate(BulkInsertDataTable1);
        }
    }
}