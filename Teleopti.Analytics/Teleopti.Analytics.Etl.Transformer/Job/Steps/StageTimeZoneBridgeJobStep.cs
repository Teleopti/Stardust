using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageTimeZoneBridgeJobStep : JobStepBase
    {
        public StageTimeZoneBridgeJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_time_zone_bridge";
            IsBusinessUnitIndependent = true;
            TimeZoneInfrastructure.AddColumnsToBridgeDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            const int bulkRowsLimit = 100000;
            bool doTruncateTable = true;
            int rowCount = 0;
            IList<TimeZoneBridge> bulkList = new List<TimeZoneBridge>();
            var transformer = new TimeZoneTransformer(DateTime.Now);

            DateTimePeriod? period = _jobParameters.StateHolder.PeriodToLoadBridgeTimeZone;
            if (!period.HasValue)
            {
                // No valid period - truncate table and skip job step
                return _jobParameters.Helper.Repository.PersistTimeZoneBridge(BulkInsertDataTable1, true);
            }

            IList<TimeZoneBridge> rootList = TimeZoneFactory.CreateTimeZoneBridgeList(period.Value, _jobParameters.IntervalsPerDay,
                                                                      _jobParameters.StateHolder.TimeZoneCollection);

            // Loop through to transform and bulk insert a certain amount of rows
            foreach (TimeZoneBridge timeZoneBridge in rootList)
            {
                bulkList.Add(timeZoneBridge);
                if (bulkList.Count % bulkRowsLimit == 0)
                {
                    BulkInsertDataTable1.Rows.Clear();
                    //Transform time zone info to Matrix format
                    transformer.TransformBridge(bulkList, BulkInsertDataTable1);
                    //Truncate (first time only) staging table & Bulk insert data to staging database
                    rowCount += _jobParameters.Helper.Repository.PersistTimeZoneBridge(BulkInsertDataTable1, doTruncateTable);

                    bulkList = new List<TimeZoneBridge>();
                    doTruncateTable = false;
                }
            }

            // Take care of the last rows that are less than bulkRowsLimit
            if (bulkList.Count > 0)
            {
                BulkInsertDataTable1.Rows.Clear();
                //Transform time zone info to Matrix format
                transformer.TransformBridge(bulkList, BulkInsertDataTable1);
                //Truncate (first time only) staging table & Bulk insert data to staging database
                rowCount += _jobParameters.Helper.Repository.PersistTimeZoneBridge(BulkInsertDataTable1, doTruncateTable);
            }


            return rowCount;

        }
    }
}