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

		public static void EtlJobIntradaySettingsDelete()
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "delete from mart.etl_job_intraday_settings";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public static IntradaySettingValue GetEtlJobIntradaySettingsValue(int businessUnitId, int dataSourceId, int detailId)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = "select target_date, target_interval from mart.etl_job_intraday_settings where business_unit_id=@business_unit_id and datasource_id=@datasource_id";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					IntradaySettingValue intradaySettingValue = null;
					sqlCommand.Parameters.AddWithValue("@business_unit_id", businessUnitId);
					sqlCommand.Parameters.AddWithValue("@datasource_id", dataSourceId);
					using (var reader = sqlCommand.ExecuteReader())
					{
						if (reader.HasRows)
						{
							reader.Read();
							intradaySettingValue = new IntradaySettingValue
							{
								TargetDate = new DateOnly(reader.GetDateTime(0)),
								TargetInterval = reader.GetInt16(1)
							};
						}
					}

					return intradaySettingValue;
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

		public static int SumFactScheduleDeviation(DateTime utcDate, string columnName)
		{
			using (var sqlConnection = connectAndOpen(InfraTestConfigReader.AnalyticsConnectionString))
			{
				string sql = $"select sum(f.{@columnName}) from mart.fact_schedule_deviation f " +
										   "inner join mart.dim_date d " +
										   "on f.shift_startdate_id = d.date_id " +
										   "where d.date_date = @date and is_logged_in = 1";
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@date", utcDate.Date);
					var deviationSum = sqlCommand.ExecuteScalar();
					return Convert.ToInt32(deviationSum == DBNull.Value ? 0 : deviationSum, CultureInfo.CurrentCulture);
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

	public class IntradaySettingValue
	{
		public DateOnly TargetDate { get; set; }
		public int TargetInterval { get; set; }
	}
}