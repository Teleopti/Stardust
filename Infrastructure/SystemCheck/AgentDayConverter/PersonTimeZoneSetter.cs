using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonTimeZoneSetter : IPersonAssignmentConverter
	{
		private const string updatePersonTimeZoneCommand = "update person set DefaultTimeZone=@timeZone where id=@personId";
		private const string updatePersonAssignmentDateCommand = "update personAssignment set [Date]=@baseDate where person=@personId";
		private const string updatePersonAssignmentAuditDateCommand = "update auditing.personAssignment_AUD set [Date]=@baseDate where person=@personId";

		public void Execute(SqlTransaction transaction, Guid personId, TimeZoneInfo timeZoneInfo)
		{
			using (var cmd = new SqlCommand(updatePersonTimeZoneCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@timeZone", timeZoneInfo.Id);
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.ExecuteNonQuery();
			}
			using (var cmd = new SqlCommand(updatePersonAssignmentDateCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.Parameters.AddWithValue("@baseDate", AgentDayConverters.DateOfUnconvertedSchedule.Date);
				cmd.ExecuteNonQuery();
			}
			using (var cmd = new SqlCommand(updatePersonAssignmentAuditDateCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.Parameters.AddWithValue("@baseDate", AgentDayConverters.DateOfUnconvertedSchedule.Date);
				cmd.ExecuteNonQuery();
			}
		}
	}
}