using System;
using System.Data;
using System.Threading;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Database
{
    internal class RepositoryStub : Teleopti.Analytics.Etl.Interfaces.Common.IScheduleRepository, ILogRepository
    {
        private readonly DataTable _logTable;
        private readonly DataTable _scheduleTable;
        private readonly DataTable _schedulePeriodsTable;

        public RepositoryStub()
        {
            _scheduleTable = NewScheduleDataTable();
            _logTable = NewLogDataTable();
            _schedulePeriodsTable = NewSchedulePeriodsDataTable();
        }

        #region IRepository Members

        public DataTable GetSchedules()
        {
            AddSchedule(1, 60);
            AddSchedule(2, 15, 180, 1430);
            AddSchedule(-1, 240); // -1 indicates new item not yet or newly persisted
            return _scheduleTable;
        }

        public DataTable GetSchedulePeriods(int scheduleId)
        {
            _schedulePeriodsTable.Rows.Clear();

            if (scheduleId == 1)
            {
                //Main job
                AddSchedulePeriod(scheduleId, "Schedule", new MinMax<int>(-7, 7));
                AddSchedulePeriod(scheduleId, "Forecast", new MinMax<int>(0, 14));
                AddSchedulePeriod(scheduleId, "Queue Statistics", new MinMax<int>(-3, 0));
                AddSchedulePeriod(scheduleId, "Agent Statistics", new MinMax<int>(-4, -1));
            }
            else
            {
                //Forecast job
                AddSchedulePeriod(scheduleId, "Forecast", new MinMax<int>(0, 14));
            }
            
            return _schedulePeriodsTable;
        }

        public void SaveSchedulePeriods(IEtlSchedule etlSchedule)
        {
            throw new NotImplementedException();
        }

        public DataTable GetLog()
        {
            AddLog(1, 60);
            AddLog(2, 180);
            return _logTable;
        }

        public int SaveLogPre()
        {
            throw new NotImplementedException();
        }

        public void SaveLogPost(IEtlLog etlLogItem, IJobResult jobResult)
        {
            throw new NotImplementedException();
        }

        public void SaveLogStepPost(IEtlLog etlLogItem, IJobStepResult jobStepResult)
        {
            throw new NotImplementedException();
        }

        public void AddJobStep(IEtlLog etlLogItem, IJobStepResult jobStepResult)
        {
            throw new NotImplementedException();
        }

    	public DataTable GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId)
    	{
    		throw new NotImplementedException();
    	}

    	//public void SaveLogPost(IEtlLog etlLogItem)
        //{
            
        //}

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static DataTable NewScheduleDataTable()
        {
            var scheduleTable = new DataTable();
            scheduleTable.Locale = Thread.CurrentThread.CurrentCulture;

            scheduleTable.Columns.Add("schedule_id", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("schedule_name", Type.GetType("System.String"));
            scheduleTable.Columns.Add("enabled", Type.GetType("System.Boolean"));
            scheduleTable.Columns.Add("schedule_type", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("occurs_daily_at", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("occurs_every_minute", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("recurring_starttime", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("recurring_endtime", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("etl_job_name", Type.GetType("System.String"));
            scheduleTable.Columns.Add("etl_relative_period_start", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("etl_relative_period_end", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("etl_datasource_id", Type.GetType("System.Int32"));
            scheduleTable.Columns.Add("description", Type.GetType("System.String"));

            return scheduleTable;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static DataTable NewSchedulePeriodsDataTable()
        {
            var schedulePeriodsTable = new DataTable();
            schedulePeriodsTable.Locale = Thread.CurrentThread.CurrentCulture;

            schedulePeriodsTable.Columns.Add("schedule_id", Type.GetType("System.Int32"));
            schedulePeriodsTable.Columns.Add("job_id", Type.GetType("System.Int32"));
            schedulePeriodsTable.Columns.Add("job_name", Type.GetType("System.String"));
            schedulePeriodsTable.Columns.Add("relative_period_start", Type.GetType("System.Int32"));
            schedulePeriodsTable.Columns.Add("relative_period_end", Type.GetType("System.Int32"));

            return schedulePeriodsTable;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static DataTable NewLogDataTable()
        {
            var logTable = new DataTable();
            logTable.Locale = Thread.CurrentThread.CurrentCulture;

            logTable.Columns.Add("schedule_id", Type.GetType("System.Int32"));
            logTable.Columns.Add("job_start_time", Type.GetType("System.DateTime"));
            logTable.Columns.Add("job_end_time", Type.GetType("System.DateTime"));

            return logTable;
        }

        private void AddSchedulePeriod(int scheduleId, string jobName, MinMax<int> relativePeriod)
        {
            DataRow dr = _schedulePeriodsTable.NewRow();
            dr["schedule_id"] = scheduleId;
            dr["job_id"] = 1;
            dr["job_name"] = jobName;
            dr["relative_period_start"] = relativePeriod.Minimum;
            dr["relative_period_end"] = relativePeriod.Maximum;
            _schedulePeriodsTable.Rows.Add(dr);
        }

        public void AddSchedule(int scheduleId, int occursDailyAt)
        {
            DataRow dr = _scheduleTable.NewRow();
            dr["schedule_id"] = scheduleId;
            dr["schedule_name"] = "OccursDaily";
            dr["enabled"] = true;
            dr["schedule_type"] = JobScheduleType.OccursDaily;
            dr["occurs_daily_at"] = occursDailyAt;
            dr["occurs_every_minute"] = 0;
            dr["recurring_starttime"] = 0;
            dr["recurring_endtime"] = 0;
            dr["etl_job_name"] = "Intraday";
            dr["etl_relative_period_start"] = -14;
            dr["etl_relative_period_end"] = 14;
            dr["etl_datasource_id"] = 1;
            dr["description"] = "Occurs daily at x.";
            _scheduleTable.Rows.Add(dr);
        }

        public void AddSchedule(int scheduleId, int occursEveryMinute, int start, int end)
        {
            DataRow dr = _scheduleTable.NewRow();
            dr["schedule_id"] = scheduleId;
            dr["schedule_name"] = "Periodic";
            dr["enabled"] = true;
            dr["schedule_type"] = JobScheduleType.Periodic;
            dr["occurs_daily_at"] = 0;
            dr["occurs_every_minute"] = occursEveryMinute;
            dr["recurring_starttime"] = start;
            dr["recurring_endtime"] = end;
            dr["etl_job_name"] = "Forecast";
            dr["etl_relative_period_start"] = -7;
            dr["etl_relative_period_end"] = 7;
            dr["etl_datasource_id"] = 1;
            dr["description"] = "Occurs daily every x minute.";

            _scheduleTable.Rows.Add(dr);
        }


        public void AddLog(int ID, int Start)
        {
            DataRow dr = _logTable.NewRow();
            dr["schedule_id"] = ID;

            var ts = new TimeSpan(0, 0, Start, 0);

            dr["job_start_time"] =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, DateTimeKind.Unspecified)
                    .Add(ts);
            dr["job_end_time"] = ((DateTime) dr["job_start_time"]).AddMinutes(3);
            _logTable.Rows.Add(dr);
        }

        #region IRepository Members


        public int SaveSchedule(IEtlSchedule etlScheduleItem)
        {
            return 1;
        }

        public void DeleteSchedule(int scheduleId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}