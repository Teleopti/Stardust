using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public abstract class PersonAssignmentDateSetterBase : IPersonAssignmentConverter
	{
		private readonly SqlTransaction _transaction;

		protected PersonAssignmentDateSetterBase(SqlTransaction transaction)
		{
			_transaction = transaction;
		}

		protected abstract string NumberOfNotConvertedCommand { get; }
		protected abstract string ReadUnconvertedSchedulesCommand { get; }
		protected abstract string UpdateAssignmentDateCommand { get; }

		public void Execute(Guid personId, TimeZoneInfo timeZone)
		{
			var dt = readSchedules(personId);
			setDateAndIncreaseVersion(dt, timeZone);
			updatePersonAssignmentRows(dt);

			checkAllConverted(personId);
		}

		private void checkAllConverted(Guid personId)
		{
			using (var cmd = new SqlCommand(NumberOfNotConvertedCommand, _transaction.Connection, _transaction))
			{
				cmd.Parameters.AddWithValue("@personId", personId);
				if ((int)cmd.ExecuteScalar() > 0)
				{
					throw new NotSupportedException("Something went wrong. There is still unconverted schedules in the database!");
				}
			}
		}

		private void updatePersonAssignmentRows(DataTable dataTable)
		{
			foreach (DataRow row in dataTable.Rows)
			{
				using (var command = new SqlCommand(UpdateAssignmentDateCommand, _transaction.Connection, _transaction))
				{
					command.CommandType = CommandType.Text;
					command.Parameters.AddWithValue("@newDate", string.Format("{0:s}", row["TheDate"]));
					command.Parameters.AddWithValue("@newVersion", row["Version"]);
					command.Parameters.AddWithValue("@id", row["Id"]);
					command.ExecuteNonQuery();
				}
			}
		}

		private static void setDateAndIncreaseVersion(DataTable dt, TimeZoneInfo timeZoneInfo)
		{
			foreach (DataRow row in dt.Rows)
			{
				var utcTime = new DateTime(((DateTime)row["Minimum"]).Ticks, DateTimeKind.Utc);
				var localDate = timeZoneInfo.SafeConvertTimeToUtc(utcTime);
				row["TheDate"] = string.Format("{0:s}", localDate.Date);
				var version = (int)row["Version"];
				row["Version"] = version + 1;
			}
		}

		private DataTable readSchedules(Guid personId)
		{
			using (var command = new SqlCommand(ReadUnconvertedSchedulesCommand, _transaction.Connection, _transaction))
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