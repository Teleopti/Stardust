﻿using NHibernate;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public static class ResetDateForAssignments
	{
		public static void ResetDateForAllAssignmentsAndAudits(this ISession session)
		{
			session.CreateSQLQuery("update PersonAssignment set TheDate=:date")
						 .SetDateTime("date", AgentDayConverters.DateOfUnconvertedSchedule)
						 .ExecuteUpdate();
			session.CreateSQLQuery("update [Auditing].PersonAssignment_AUD set TheDate=:date")
						 .SetDateTime("date", AgentDayConverters.DateOfUnconvertedSchedule)
						 .ExecuteUpdate();
		}
	}
}