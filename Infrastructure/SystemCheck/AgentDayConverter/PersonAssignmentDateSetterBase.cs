using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public abstract class PersonAssignmentDateSetterBase : IPersonAssignmentConverter
	{
		protected abstract string NumberOfNotConvertedCommand { get; }
		protected abstract string ReadUnconvertedSchedulesCommand { get; }
		protected abstract string UpdateAssignmentDateCommand { get; }

		public void Execute(SqlTransaction transaction, Guid personId, TimeZoneInfo timeZone)
		{
			var dt = readSchedules(transaction, personId);
			setDate(dt, timeZone);
			updatePersonAssignmentRows(transaction, dt);

			checkAllConverted(transaction, personId);
		}

		private void checkAllConverted(SqlTransaction transaction, Guid personId)
		{
			using (var cmd = new SqlCommand(NumberOfNotConvertedCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.Parameters.AddWithValue("@baseDate", AgentDayConverters.DateOfUnconvertedSchedule.Date);
				if ((int)cmd.ExecuteScalar() > 0)
				{
					throw new NotSupportedException("Something went wrong. There is still unconverted schedules in the database!");
				}
			}
		}

		private void updatePersonAssignmentRows(SqlTransaction transaction, DataTable dataTable)
		{
			foreach (DataRow row in dataTable.Rows)
			{
				using (var command = new SqlCommand(UpdateAssignmentDateCommand, transaction.Connection, transaction))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.AddWithValue("@newDate", string.Format("{0:s}", row["Date"]));
					command.Parameters.AddWithValue("@id", row["Id"]);
					command.ExecuteNonQuery();
				}
			}
		}

		private static void setDate(DataTable dt, TimeZoneInfo timeZoneInfo)
		{
			foreach (DataRow row in dt.Rows)
			{
				var utcTime = new DateTime(((DateTime)row["Minimum"]).Ticks, DateTimeKind.Utc);
				var localDate = TimeZoneHelper.ConvertFromUtc(utcTime, timeZoneInfo);
				row["Date"] = string.Format("{0:s}", localDate.Date);
			}
		}

		private DataTable readSchedules(SqlTransaction transaction, Guid personId)
		{
			using (var command = new SqlCommand(ReadUnconvertedSchedulesCommand, transaction.Connection, transaction))
			{
				command.Parameters.AddWithValue("@personId", personId);
				command.Parameters.AddWithValue("@baseDate", AgentDayConverters.DateOfUnconvertedSchedule.Date);
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