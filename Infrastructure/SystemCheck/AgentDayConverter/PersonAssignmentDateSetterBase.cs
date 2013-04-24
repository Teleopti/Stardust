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

				var rows = _personAssignmentCommon.ReadRows(connection, ReadCommand);
				_personAssignmentCommon.SetFields(rows);
				updatePersonAssignmentRows(rows, connection);



				//fix - throw error here if not 0
				rows = _personAssignmentCommon.ReadRows(connection, NumberOfNotConvertedCommand);

			}
		}

		private void updatePersonAssignmentRows(IEnumerable<DataRow> rows, SqlConnection connection)
		{
			foreach (var dataRow in rows)
			{
				using (var command = new SqlCommand())
				{
					command.CommandType = CommandType.Text;
					command.Connection = connection;
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