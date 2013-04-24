

using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
	public interface IResetDateOnlyAfterChangedTimeZone
	{
		bool ResetFor(IPerson person);
	}

	public class ResetDateOnlyAfterChangedTimeZone : IResetDateOnlyAfterChangedTimeZone
	{
		public bool ResetFor(IPerson person)
		{
			var resetter = new PersonAssignmentDateResetter();
			var connStr = UnitOfWorkFactory.CurrentUnitOfWorkFactory().LoggedOnUnitOfWorkFactory().ConnectionString;
			resetter.ExecuteFor(person, connStr);
			var builder = new SqlConnectionStringBuilder(connStr);
			//IPersonAssignmentConverter assignmentConverter = new PersonAssignmentDateSetter();
			//assignmentConverter.Execute(builder);
			//assignmentConverter = new PersonAssignmentAuditDateSetter();
			//assignmentConverter.Execute(builder);

			return true;
		}
	}
}