using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonTimeZoneSetter : IPersonAssignmentConverter
	{
		private const string updateCommand = "update person set DefaultTimeZone=@timeZone where id=@personId";
		
		public void Execute(SqlTransaction transaction, Guid personId, TimeZoneInfo timeZoneInfo)
		{
			using (var cmd = new SqlCommand(updateCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@timeZone", timeZoneInfo.Id);
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.ExecuteNonQuery();
			}
		}
	}
}