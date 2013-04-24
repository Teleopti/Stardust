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
		private readonly IPersonAssignmentCommon _personAssignmentCommon = new PersonAssignmentCommon();

		protected abstract string NumberOfNotConvertedCommand { get; }
		protected abstract string ReadCommand { get; }
		protected abstract string UpdateAssignmentDate { get; }

		public int Execute(SqlConnectionStringBuilder connectionStringBuilder)
		{
			var connectionString = connectionStringBuilder.ToString();

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				if (!_personAssignmentCommon.TheDateFieldExists(connection, NumberOfNotConvertedCommand))
					return 0;

				int batchRows;
				do
				{
					batchRows = runOneBatch(connection);
				} while (batchRows > 0);


				var rows = _personAssignmentCommon.ReadRows(connection, NumberOfNotConvertedCommand, null);
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

				rows = _personAssignmentCommon.ReadRows(connection, ReadCommand, transaction);
				_personAssignmentCommon.SetFields(rows);
				updatePersonAssignmentRows(rows, connection, transaction);

				transaction.Commit();
			}
			return rows.Count;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private void updatePersonAssignmentRows(IEnumerable<DataRow> rows, SqlConnection connection, SqlTransaction transaction)
		{
			foreach (var dataRow in rows)
			{
				using (var command = new SqlCommand())
				{
					command.CommandType = CommandType.Text;
					command.Connection = connection;
					command.Transaction = transaction;
					command.CommandText = UpdateAssignmentDate;
					command.Parameters.AddWithValue("@newDate", string.Format("{0:s}", dataRow.Field<DateTime>("TheDate")));
					command.Parameters.AddWithValue("@newVersion", dataRow.Field<int>("Version"));
					command.Parameters.AddWithValue("@id", dataRow.Field<Guid>("Id"));
					command.ExecuteNonQuery();
				}
			}
		}
	}
}