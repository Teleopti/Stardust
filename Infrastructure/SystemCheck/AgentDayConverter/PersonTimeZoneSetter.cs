using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class PersonTimeZoneSetter : IPersonAssignmentConverter
	{
		private readonly SqlTransaction _transaction;
		private const string updateCommand = "update person set DefaultTimeZone=@timeZone where id=@personId";

		public PersonTimeZoneSetter(SqlTransaction transaction)
		{
			_transaction = transaction;
		}

		public void Execute(Guid personId, TimeZoneInfo timeZoneInfo)
		{
			using (var cmd = new SqlCommand(updateCommand, _transaction.Connection, _transaction))
			{
				cmd.Parameters.AddWithValue("@timeZone", timeZoneInfo.Id);
				cmd.Parameters.AddWithValue("@personId", personId);
				cmd.ExecuteNonQuery();
			}
		}
	}
}