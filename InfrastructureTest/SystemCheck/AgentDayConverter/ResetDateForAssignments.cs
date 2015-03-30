using NHibernate;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public static class ResetDateForAssignments
	{
		public static void ResetDateForAllAssignmentsAndAudits(this ISession session)
		{
			session.CreateSQLQuery("update PersonAssignment set [Date]=:date")
						 .SetDateOnly("date", PersonAssignmentDateSetter.DateOfUnconvertedSchedule)
						 .ExecuteUpdate();
			session.CreateSQLQuery("update [Auditing].PersonAssignment_AUD set [Date]=:date")
						 .SetDateOnly("date", PersonAssignmentDateSetter.DateOfUnconvertedSchedule)
						 .ExecuteUpdate();
		}
	}
}