using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class RemoveEmptyAssignments : IPersonAssignmentConverter
	{
		private const string removeEmptyAssignmentsCommand =
			@"
delete from PersonAssignment
where id in 
(
select pa.id from personassignment pa
left outer join ShiftLayer sl on pa.Id=sl.Parent
where not exists(select 1 from ShiftLayer where Parent=pa.Id)
)
and Date = @baseDate";

		public void Execute(SqlTransaction transaction, Guid personId, TimeZoneInfo timeZoneInfo)
		{
			using (var cmd = new SqlCommand(removeEmptyAssignmentsCommand, transaction.Connection, transaction))
			{
				cmd.Parameters.AddWithValue("@baseDate", AgentDayConverters.DateOfUnconvertedSchedule.Date);
				cmd.ExecuteNonQuery();
			}
		}
	}
}