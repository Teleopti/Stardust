﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	public class SqlCommands
	{
		public static int CountIntervalsPerLocalDate(IPerson person, DateTime datelocal)
		{
			using (var sqlConnection = connectAndOpen(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
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

		public static int RowsInFactSchedule(DateTime? startTime = null)
		{
			using (var sqlConnection = connectAndOpen(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
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

		public static int SumFactScheduleDeviation(IPerson person, DateTime datelocal, string columnName)
		{
			using (var sqlConnection = connectAndOpen(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
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
			using (var sqlConnection = connectAndOpen(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
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
			using (var sqlConnection = connectAndOpen(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix))
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