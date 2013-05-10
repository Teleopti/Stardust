using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public static class ExecuteTarget
	{
		public static void ExecuteConverterAndWrapInTransaction(this IPersonAssignmentConverter converter, Guid personId, TimeZoneInfo timeZone)
		{
			using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
			{
				conn.Open();
				using (var tran = conn.BeginTransaction())
				{
					converter.Execute(tran, personId, timeZone);
					tran.Commit();
				}
			}
		}
	}
}