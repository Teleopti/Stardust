using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Infrastructure
{
	internal class RepositoryStub : IJobScheduleRepository, IJobLogRepository
	{
		private readonly DataTable _logTable;
		private readonly IList<IEtlJobSchedule> _jobSchedules;

		public RepositoryStub()
		{
			_logTable = NewLogDataTable();
			_jobSchedules = new List<IEtlJobSchedule>();
		}

		public IList<IEtlJobSchedule> GetSchedules(IEtlJobLogCollection etlJobLogCollection, DateTime serverStartTime)
		{
			AddSchedule(1, 60, etlJobLogCollection);
			AddSchedule(2, 15, 180, 1430, etlJobLogCollection);
			AddSchedule(-1, 240, etlJobLogCollection); // -1 indicates new item not yet or newly persisted
			AddSchedule(3);
			return _jobSchedules;
		}

		public IList<IEtlJobRelativePeriod> GetSchedulePeriods(int scheduleId)
		{
			var jobPeriods = new List<IEtlJobRelativePeriod>();
			if (scheduleId == 1)
			{
				//Main job
				jobPeriods.Add(AddSchedulePeriod(JobCategoryType.Schedule, new MinMax<int>(-7, 7)));
				jobPeriods.Add(AddSchedulePeriod(JobCategoryType.Forecast, new MinMax<int>(0, 14)));
				jobPeriods.Add(AddSchedulePeriod(JobCategoryType.QueueStatistics, new MinMax<int>(-3, 0)));
				jobPeriods.Add(AddSchedulePeriod(JobCategoryType.AgentStatistics, new MinMax<int>(-4, -1)));
			}
			else
			{
				//Forecast job
				jobPeriods.Add(AddSchedulePeriod(JobCategoryType.Forecast, new MinMax<int>(0, 14)));
			}
			
			return jobPeriods;
		}

		public void SaveSchedulePeriods(IEtlJobSchedule etlJobSchedule)
		{
			throw new NotImplementedException();
		}

		public void SetDataMartConnectionString(string connectionString)
		{
			throw new NotImplementedException();
		}

		public void ToggleScheduleJobEnabledState(int scheduleId)
		{
			throw new NotImplementedException();
		}

		public DataTable GetLog()
		{
			AddLog(1, 60);
			AddLog(2, 180);
			return _logTable;
		}

		public int SaveLogPre(int scheduleId)
		{
			throw new NotImplementedException();
		}

		public void SaveLogPost(IEtlJobLog etlJobLogItem, IJobResult jobResult)
		{
			throw new NotImplementedException();
		}

		public void SaveLogStepPost(IEtlJobLog etlJobLogItem, IJobStepResult jobStepResult)
		{
			throw new NotImplementedException();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private static DataTable NewLogDataTable()
		{
			var logTable = new DataTable {Locale = Thread.CurrentThread.CurrentCulture};

			logTable.Columns.Add("schedule_id", typeof(int));
			logTable.Columns.Add("tenant_name", typeof(string));
			logTable.Columns.Add("job_start_time", typeof(DateTime));
			logTable.Columns.Add("job_end_time", typeof(DateTime));

			return logTable;
		}

		private IEtlJobRelativePeriod AddSchedulePeriod(JobCategoryType jobCategoryType, MinMax<int> relativePeriod)
		{
			return new EtlJobRelativePeriod(relativePeriod, jobCategoryType);
		}

		public void AddSchedule(int scheduleId, int occursDailyAt, IEtlJobLogCollection etlJobLogCollection)
		{
			_jobSchedules.Add(new EtlJobSchedule(
				scheduleId,
				"OccursDaily",
				true,
				occursDailyAt,
				"Intraday",
				1,
				"Occurs daily at x.",
				etlJobLogCollection,
				GetSchedulePeriods(scheduleId),
				"Teleopti WFM"));
		}

		public void AddSchedule(int scheduleId, int occursEveryMinute, int start, int end, IEtlJobLogCollection etlJobLogCollection)
		{
			_jobSchedules.Add(new EtlJobSchedule(
				scheduleId,
				"Periodic",
				true,
				occursEveryMinute,
				start,
				end,
				"Forecast",
				1,
				"Occurs daily every x minute.",
				etlJobLogCollection,
				DateTime.Now,
				GetSchedulePeriods(scheduleId),
				"Teleopti WFM"));
		}

		public void AddSchedule(int scheduleId)
		{
			_jobSchedules.Add(new EtlJobSchedule(
				scheduleId,
				"Manual ETL",
				"Intraday",
				true,
				1,
				"Manual ETL",
				new DateTime(2017, 12, 12, 15, 33, 0),
				null,
				"Teleopti WFM"));
		}

		public void AddLog(int id, int start)
		{
			DataRow dr = _logTable.NewRow();
			dr["schedule_id"] = id;

			var ts = new TimeSpan(0, 0, start, 0);

			dr["job_start_time"] =
				new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, DateTimeKind.Unspecified)
					.Add(ts);
			dr["job_end_time"] = ((DateTime) dr["job_start_time"]).AddMinutes(3);
			_logTable.Rows.Add(dr);
		}

		#region IRepository Members

		public int SaveSchedule(IEtlJobSchedule jobSchedule)
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