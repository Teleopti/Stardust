﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public abstract class PersonAssignmentDateSetterBase : IPersonAssignmentConverter
	{
		protected abstract string NumberOfNotConvertedCommand { get; }
		protected abstract string ReadUnconvertedSchedulesCommand { get; }
		protected abstract string UpdateAssignmentDateCommand { get; }

		public void Execute(SqlConnectionStringBuilder connectionStringBuilder)
		{
			var connectionString = connectionStringBuilder.ToString();

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				var dt = readSchedules(connection);
				setDateAndIncreaseVersion(dt);
				updatePersonAssignmentRows(dt, connection);

				checkAllConverted(connection);
			}
		}

		private void checkAllConverted(SqlConnection sqlConnection)
		{
			if ((int) new SqlCommand(NumberOfNotConvertedCommand, sqlConnection).ExecuteScalar() > 0)
			{
				throw new NotSupportedException("Something went wrong. There is still unconverted schedules in the database!");
			}
		}

		private void updatePersonAssignmentRows(DataTable dataTable, SqlConnection connection)
		{
			foreach (DataRow row in dataTable.Rows)
			{
				using (var command = new SqlCommand())
				{
					command.CommandType = CommandType.Text;
					command.Connection = connection;
					command.CommandText = UpdateAssignmentDateCommand;
					command.Parameters.AddWithValue("@newDate", string.Format("{0:s}", row["TheDate"]));
					command.Parameters.AddWithValue("@newVersion", row["Version"]);
					command.Parameters.AddWithValue("@id", row["Id"]);
					command.ExecuteNonQuery();
				}
			}
		}

		private static void setDateAndIncreaseVersion(DataTable dt)
		{
			foreach (DataRow row in dt.Rows)
			{
				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById((string) row["DefaultTimeZone"]);
				var utcTime = new DateTime(((DateTime) row["Minimum"]).Ticks, DateTimeKind.Utc);
				var localDate = timeZoneInfo.SafeConvertTimeToUtc(utcTime);
				row["TheDate"] = string.Format("{0:s}", localDate.Date);
				var version = (int)row["Version"];
				row["Version"] = version + 1;
			}
		}

		private DataTable readSchedules(SqlConnection connection)
		{
			using (var command = new SqlCommand(ReadUnconvertedSchedulesCommand, connection))
			{
				using (var dataAdapter = new SqlDataAdapter(command))
				{
					var dt = new DataTable();
					dataAdapter.Fill(dt);
					return dt;
				}
			}
		}
	}
}