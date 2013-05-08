using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonTimeZoneSetter : IPersonAssignmentConverter
	{
		private const string updatePersonTimeZoneCommand = "update person set DefaultTimeZone=@timeZone where id=@personId";
		private readonly string updatePersonAssignmentDateCommand = "update personAssignment set TheDate='" + AgentDayConverters.DateOfUnconvertedSchedule + "' where person=@personId";
		private readonly string updatePersonAssignmentAuditDateCommand = "update auditing.personAssignment_AUD set TheDate='" + AgentDayConverters.DateOfUnconvertedSchedule + "' where person=@personId";
		
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
				cmd.ExecuteNonQuery();
			}
			using (var cmd = new SqlCommand(updatePersonAssignmentAuditDateCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.ExecuteNonQuery();
			}
		}
	}
}