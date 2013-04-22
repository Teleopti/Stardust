using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IPersonAssignmentCommon
	{
		IList<DataRow> ReadRows(SqlConnection connection, string readCommand, SqlTransaction transaction);

		void SetFields(IEnumerable<DataRow> rows);

		bool TheDateFieldExists(SqlConnection connection, string numberOfNotConvertedCommand);
	}
}