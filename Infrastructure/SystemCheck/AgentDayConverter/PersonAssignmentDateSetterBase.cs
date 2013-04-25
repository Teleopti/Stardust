using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public abstract class PersonAssignmentDateSetterBase : IPersonAssignmentConverter
	{
		private readonly SqlConnectionStringBuilder _tempShouldNotBeLikeThis;

		protected PersonAssignmentDateSetterBase(SqlConnectionStringBuilder tempShouldNotBeLikeThis)
		{
			_tempShouldNotBeLikeThis = tempShouldNotBeLikeThis;
		}

		protected abstract string NumberOfNotConvertedCommand { get; }
		protected abstract string ReadUnconvertedSchedulesCommand { get; }
		protected abstract string UpdateAssignmentDateCommand { get; }

		public void Execute(Guid personId)
		{
			var connectionString = _tempShouldNotBeLikeThis.ToString();

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				var dt = readSchedules(connection, personId);
				setDateAndIncreaseVersion(dt);
				updatePersonAssignmentRows(dt, connection);

				checkAllConverted(connection, personId);
			}
		}

		private void checkAllConverted(SqlConnection sqlConnection, Guid personId)
		{
			using (var cmd = new SqlCommand(NumberOfNotConvertedCommand, sqlConnection))
			{
				cmd.Parameters.AddWithValue("@personId", personId);
				if ((int)cmd.ExecuteScalar() > 0)
				{
					throw new NotSupportedException("Something went wrong. There is still unconverted schedules in the database!");
				}				
			}
		}

		private void updatePersonAssignmentRows(DataTable dataTable, SqlConnection connection)
		{
			foreach (DataRow row in dataTable.Rows)
			{
				using (var command = new SqlCommand(UpdateAssignmentDateCommand, connection))
				{
					command.CommandType = CommandType.Text;
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

		private DataTable readSchedules(SqlConnection connection, Guid personId)
		{
			using (var command = new SqlCommand(ReadUnconvertedSchedulesCommand, connection))
			{
				command.Parameters.AddWithValue("@personId", personId);
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