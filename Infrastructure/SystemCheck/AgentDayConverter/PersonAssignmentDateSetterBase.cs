using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public abstract class PersonAssignmentDateSetterBase : IPersonAssignmentConverter
	{
		private const string numberOfNotConvertedCommand
		= "select COUNT(*) as cnt from dbo.PersonAssignment where TheDate < '1850-01-01'";

		private const string readCommand = "select pa.Id, p.DefaultTimeZone, pa.Minimum, pa.TheDate, pa.Version " +
		                                   "from dbo.PersonAssignment pa " +
		                                   "inner join Person p on pa.Person = p.id " +
		                                   "where pa.TheDate = '1800-1-1'";

		private readonly IPersonAssignmentCommon _personAssignmentCommon = new PersonAssignmentCommon();

		public int Execute(SqlConnectionStringBuilder connectionStringBuilder)
		{
			string connectionString = connectionStringBuilder.ToString();
			if (string.IsNullOrEmpty(connectionString))
			{
				return 0;
			}

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				if (!_personAssignmentCommon.TheDateFieldExists(connection, numberOfNotConvertedCommand))
					return 0;

				int batchRows;
				do
				{
					batchRows = runOneBatch(connection);
				} while (batchRows > 0);


				var rows = _personAssignmentCommon.ReadRows(connection, numberOfNotConvertedCommand, null);
				var rowsLeft = rows[0].Field<int>("cnt");
				if (rowsLeft > 0)
				{
					return 1;
				}
			}
			return 0;
		}

		private int runOneBatch(SqlConnection connection)
		{
			IList<DataRow> rows;
			using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
			{

				rows = _personAssignmentCommon.ReadRows(connection, readCommand, transaction);
				_personAssignmentCommon.SetFields(rows);
				updatePersonAssignmentRows(rows, connection, transaction);

				transaction.Commit();
			}
			return rows.Count;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private static void updatePersonAssignmentRows(IEnumerable<DataRow> rows, SqlConnection connection, SqlTransaction transaction)
		{
			using (var command = new SqlCommand())
			{
				command.CommandType = CommandType.Text;
				command.Connection = connection;
				command.Transaction = transaction;
				foreach (var dataRow in rows)
				{
					var commandText = new StringBuilder()
						.AppendLine("update dbo.PersonAssignment")
						.AppendLine("set TheDate = '" + string.Format("{0:s}", dataRow.Field<DateTime>("TheDate")) + "'")
						.AppendLine(", Version = " + dataRow.Field<int>("Version"))
						.AppendLine("where Id='" + dataRow.Field<Guid>("Id") + "'");

					command.CommandText = commandText.ToString();
					command.ExecuteNonQuery();
				}
			}
		}
	}
}