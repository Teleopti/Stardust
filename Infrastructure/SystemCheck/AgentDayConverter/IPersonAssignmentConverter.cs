using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	/// <summary>
	/// Used to identify a command in the Database
	/// </summary>
	public interface IPersonAssignmentConverter
	{
		/// <summary>
		/// Executes against the connectionstring
		/// </summary>
		/// <returns></returns>
		void Execute(SqlTransaction transaction, Guid personId, TimeZoneInfo timeZoneInfo);
	}
}