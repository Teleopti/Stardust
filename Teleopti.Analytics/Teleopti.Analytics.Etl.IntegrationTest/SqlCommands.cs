using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	public class SqlCommands
	{
		public static int CountIntervalsPerLocalDate(IPerson person, DateTime datelocal)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select count(1) from mart.fact_schedule_deviation f " +
				                           "inner join mart.dim_person p on f.person_id = p.person_id " +
				                           "inner join mart.dim_time_zone tz on p.time_zone_id = tz.time_zone_id " +
				                           "join mart.bridge_time_zone btz on p.time_zone_id = btz.time_zone_id " +
				                           "and f.shift_startdate_id = btz.date_id " +
				                           "and f.shift_startinterval_id = btz.interval_id " +
				                           "join mart.dim_date d " +
				                           "on btz.local_date_id = d.date_id " +
				                           "where d.date_date = @date and p.person_code = @personId";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@date", datelocal.Date);
					sqlCommand.Parameters.AddWithValue("@personId", person.Id.GetValueOrDefault());
					return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
				}
			}
		}

		public static int DataSourceIdGet(string datasourceName)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select datasource_id from mart.sys_datasource where datasource_name=@datasourceName";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@datasourceName", datasourceName);
					return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
				}
			}
		}

		public static int TimezoneIdGet(string timezoneName)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select time_zone_id from mart.dim_time_zone where time_zone_code=@timezoneName";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@timezoneName", timezoneName);
					return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
				}
			}
		}
		public static void EtlJobIntradaySettingsReset(DateTime targetDate)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "update mart.etl_job_intraday_settings set target_date=@target_date";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@target_date", targetDate);
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public static int RowsInFactSchedule(DateTime? startTime = null)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select count(1) from mart.fact_schedule";
				if (startTime.HasValue)
					sql += " where shift_starttime=@startTime";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					if (startTime.HasValue)
						sqlCommand.Parameters.AddWithValue("@startTime", startTime.Value);
					return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
				}
			}
		}

		public static int EtlErrors()
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select count(1) from mart.etl_jobstep_error";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
				}
			}
		}

		public static int MaxIntervalLogObjectDetail(int detailId, int dataSourceId)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select int_value from dbo.log_object_detail od inner join mart.sys_datasource ds on od.log_object_id=ds.log_object_id " +
								"where ds.datasource_id=@dataSourceId and detail_id=@detailId";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@detailId", detailId);
					sqlCommand.Parameters.AddWithValue("@dataSourceId", dataSourceId);
					return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
				}
			}
		}

		public static bool IntradayDetailSynced(int detailId, int dataSourceId)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select count(1) from dbo.log_object_detail od inner join mart.sys_datasource ds on od.log_object_id=ds.log_object_id " +
								"inner join mart.etl_job_intraday_settings s on s.detail_id=od.detail_id and s.datasource_id=ds.datasource_id " +
								"where ds.datasource_id=@dataSourceId and od.detail_id=@detailId";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@detailId", detailId);
					sqlCommand.Parameters.AddWithValue("@dataSourceId", dataSourceId);
					var ret = Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
					if (ret == 1)
						return true;
					else
						return false;
				}
			}
		}

		public static int SumFactScheduleDeviation(IPerson person, DateTime datelocal, string columnName)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = string.Format("select sum(f.{0}) from mart.fact_schedule_deviation f " +
				                           "inner join mart.dim_person p on f.person_id = p.person_id " +
				                           "inner join mart.dim_time_zone tz on p.time_zone_id = tz.time_zone_id " +
				                           "join mart.bridge_time_zone btz on p.time_zone_id = btz.time_zone_id " +
				                           "and f.shift_startdate_id = btz.date_id " +
				                           "and f.shift_startinterval_id = btz.interval_id " +
				                           "join mart.dim_date d " +
				                           "on btz.local_date_id = d.date_id " +
				                           "where d.date_date = @date and p.person_code = @personId",
				                           columnName);
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@date",datelocal.Date);
					sqlCommand.Parameters.AddWithValue("@personId", person.Id.GetValueOrDefault());
					return Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
				}
			}
		}

		public static DataTable ReportDataAgentScheduleAdherence(DateTime date_from, DateTime date_to, int adherence_id, IPerson person, string timeZoneId)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				const string reportResourceKey = "ResReportAdherencePerDay";
				var dtResult = new DataSet();
				using (var command = sqlConnection.CreateCommand())
				{
					var sqlAdapter = new SqlDataAdapter(command);
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "mart.report_data_agent_schedule_adherence_for_test";
					command.Parameters.AddWithValue("@date_from", date_from);
					command.Parameters.AddWithValue("@date_to", date_to);
					command.Parameters.AddWithValue("@adherence_id", adherence_id);
					command.Parameters.AddWithValue("@agent_code", person.Id);
					command.Parameters.AddWithValue("@time_zone_code", timeZoneId);
					command.Parameters.AddWithValue("@report_resource_key", reportResourceKey);
					sqlAdapter.Fill(dtResult);
					return dtResult.Tables[0];
				}
			}
		}

		public static DataTable ReportDataScheduledTimePerAgent(DateTime date_from, DateTime date_to, int adherence_id, IPerson person, string timeZoneId, string activity)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				var dtResult = new DataSet();
				const string reportResourceKey = "ResReportScheduledTimePerAgent";
				using (SqlCommand command = sqlConnection.CreateCommand())
				{
					SqlDataAdapter sqlAdapter = new SqlDataAdapter(command);
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "mart.report_data_agent_schedule_adherence_for_test";
					command.Parameters.AddWithValue("@date_from", date_from);
					command.Parameters.AddWithValue("@date_to", date_to);
					command.Parameters.AddWithValue("@adherence_id", adherence_id);
					command.Parameters.AddWithValue("@agent_code", person.Id);
					command.Parameters.AddWithValue("@time_zone_code", timeZoneId);
					command.Parameters.AddWithValue("@report_resource_key", reportResourceKey);
					command.Parameters.AddWithValue("@activity_set", activity);
					sqlAdapter.Fill(dtResult);
					return dtResult.Tables[0];
				}
			}
		}

		private static SqlConnection connectAndOpen(string connectionString)
		{
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}
	}
}