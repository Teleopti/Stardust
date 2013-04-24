using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public abstract class PersonAssignmentDateSetterBase : IPersonAssignmentConverter
	{
		private readonly IPersonAssignmentCommon _personAssignmentCommon = new PersonAssignmentCommon();

		protected abstract string NumberOfNotConvertedCommand { get; }
		protected abstract string ReadCommand { get; }
		protected abstract string UpdateAssignmentDate { get; }

		public void Execute(SqlConnectionStringBuilder connectionStringBuilder)
		{
			var connectionString = connectionStringBuilder.ToString();

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				int batchRows;
				do
				{
					batchRows = runOneBatch(connection);
				} while (batchRows > 0);


				//fix - throw error here
				var rows = _personAssignmentCommon.ReadRows(connection, NumberOfNotConvertedCommand, null);

			}
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