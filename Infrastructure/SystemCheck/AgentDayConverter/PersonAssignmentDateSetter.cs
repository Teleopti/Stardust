using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonAssignmentDateSetter
	{
		public static readonly DateOnly DateOfUnconvertedSchedule = new DateOnly(1800, 1, 1);
		public static readonly DateTime DateOfUnconvertedSchedule2 = new DateOnly(1800, 1, 1).Date;

		private string numberOfNotConvertedCommand = 
			@"select COUNT(*) as cnt from dbo.PersonAssignment pa
							where pa.[Date]=@baseDate
							and pa.Person=@personId";

		private const string readUnconvertedSchedulesCommand =
			@"select pa.Id, pa.Date, min(l.Minimum) as minimum 
				       from dbo.PersonAssignment pa
				       inner join Person p on pa.Person = p.id
							 inner join ShiftLayer l on l.Parent = pa.Id
							 where pa.[Date]=@baseDate
				       and p.Id=@personId 
				       group by pa.Id, pa.Date";

		private const string updateAssignmentDateCommand = 
			@"update dbo.PersonAssignment 
							 set [Date]=@newDate, Version=Version+1 
				       where Id=@id";

		public void Execute(SqlTransaction transaction, Guid personId, TimeZoneInfo timeZone)
		{
			var dt = readSchedules(transaction, personId);
			setDate(dt, timeZone);
			updatePersonAssignmentRows(transaction, dt);

			checkAllConverted(transaction, personId);
		}

		private void checkAllConverted(SqlTransaction transaction, Guid personId)
		{
			using (var cmd = new SqlCommand(numberOfNotConvertedCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.Parameters.AddWithValue("@baseDate", DateOfUnconvertedSchedule.Date);
				if ((int)cmd.ExecuteScalar() > 0)
				{
					throw new NotSupportedException("Something went wrong. There is still unconverted schedules in the database!");
				}
			}
		}

		private static void updatePersonAssignmentRows(SqlTransaction transaction, DataTable dataTable)
		{
			foreach (DataRow row in dataTable.Rows)
			{
				using (var command = new SqlCommand(updateAssignmentDateCommand, transaction.Connection, transaction))
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

		private static DataTable readSchedules(SqlTransaction transaction, Guid personId)
		{
			using (var command = new SqlCommand(readUnconvertedSchedulesCommand, transaction.Connection, transaction))
			{
				command.Parameters.AddWithValue("@personId", personId);
				command.Parameters.AddWithValue("@baseDate", DateOfUnconvertedSchedule.Date);
				using (var dataAdapter = new SqlDataAdapter(command))
				{
					var dt = new DataTable();
					dataAdapter.Fill(dt);
					return dt;
				}
			}
		}

		public void InjectCheckSqlStatementForTest(string injectedSql)
		{
			numberOfNotConvertedCommand = injectedSql;
		}
	}
}