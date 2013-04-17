using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Teleopti.Support.Security
{
	public class PersonAssignmentDateSetter : ICommandLineCommand
	{
		private const string numberOfNotConvertedCommand
			= "select COUNT(*) as cnt from dbo.PersonAssignment where TheDate < '1850-01-01'";

		private readonly string _readCommand = new StringBuilder()
				.AppendLine("select top 100")
				.AppendLine("Pa.Id, DefaultTimeZone, Minimum, TheDate")
				.AppendLine("from dbo.PersonAssignment pa")
				.AppendLine("inner join Person p on pa.Person = p.id")
				.AppendLine("where TheDate = '1800-01-01'")
				.ToString();

		private readonly PersonAssignmentCommon _personAssignmentCommon = new PersonAssignmentCommon();

		public int Execute(CommandLineArgument commandLineArgument)
		{
			using (var connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
			{
				connection.Open();
				if (!_personAssignmentCommon.TheDateFieldExists(connection, numberOfNotConvertedCommand))
					return 0;

				IList<DataRow> rows = _personAssignmentCommon.ReadRows(connection, numberOfNotConvertedCommand);

				Console.WriteLine(string.Concat("Found ", rows[0].Field<int>("cnt"), " non converted person assignments"));

				int total = 0;
				do
				{
					total += runOneBatch(connection);

					Console.Write(string.Concat("Rows updated = ", total));
					Console.SetCursorPosition(0, 1);

				} while (rows.Count > 0);

				Console.WriteLine();

				rows = _personAssignmentCommon.ReadRows(connection, numberOfNotConvertedCommand);
				if (rows.Count > 0)
				{
					Console.WriteLine("There is still " + rows.Count + " non converted person assignments in db.");
					return 1;
				}
			}

			Console.WriteLine();

			return 0;
		}

		private int runOneBatch(SqlConnection connection)
		{
			IList<DataRow> rows;
			
			using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
			{
				rows = _personAssignmentCommon.ReadRows(connection, _readCommand);
				_personAssignmentCommon.SetFields(rows);
				updatePersonAssignmentRows(rows, connection);

				transaction.Commit();
			}
			connection.Close();
			return rows.Count;
		}

		private static void updatePersonAssignmentRows(IEnumerable<DataRow> rows, SqlConnection connection)
		{
			using (var command = new SqlCommand())
			{
				command.CommandType = CommandType.Text;
				command.Connection = connection;
				foreach (var dataRow in rows)
				{
					var commandText = new StringBuilder()
						.AppendLine("update dbo.PersonAssignment")
						.AppendLine("set TheDate = '" + dataRow.Field<DateTime>("TheDate"))
						.AppendLine("where Id='" + dataRow.Field<Guid>("Id") + "'");

					command.CommandText = commandText.ToString();
					command.ExecuteNonQuery();
				}
			}
		}
	}
}