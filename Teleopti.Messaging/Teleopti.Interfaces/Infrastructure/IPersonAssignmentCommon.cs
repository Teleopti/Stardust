using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// Common code for converting to dateonly
	/// </summary>
	public interface IPersonAssignmentCommon
	{
		/// <summary>
		/// Read
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="readCommand"></param>
		/// <returns></returns>
		IList<DataRow> ReadRows(SqlConnection connection, string readCommand);

		/// <summary>
		/// Modify
		/// </summary>
		/// <param name="rows"></param>
		void SetFields(IEnumerable<DataRow> rows);

		/// <summary>
		/// Check if correct version
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="numberOfNotConvertedCommand"></param>
		/// <returns></returns>
		bool TheDateFieldExists(SqlConnection connection, string numberOfNotConvertedCommand);
	}
}