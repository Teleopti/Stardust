using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
	public class PersonAssignmentDateSetter : IPersonAssignmentConverter
	{
		private const string numberOfNotConvertedCommand
			= "select COUNT(*) as cnt from dbo.PersonAssignment where TheDate < '1850-01-01'";

		private readonly string _readCommand = new StringBuilder()
				.AppendLine("select top 100")
				.AppendLine("Pa.Id, DefaultTimeZone, Minimum, TheDate, Pa.Version")
				.AppendLine("from dbo.PersonAssignment pa")
				.AppendLine("inner join Person p on pa.Person = p.id")
				.AppendLine("where TheDate = '1800-01-01'")
				.ToString();

		private readonly PersonAssignmentCommon _personAssignmentCommon = new PersonAssignmentCommon();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.Write(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
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

				IList<DataRow> rows = _personAssignmentCommon.ReadRows(connection, numberOfNotConvertedCommand, null);

				Console.WriteLine(string.Concat("Found ", rows[0].Field<int>("cnt"), " non converted person assignments"));

				connection.Close();

				int total = 0;
				int batchRows;
				do
				{
					batchRows = runOneBatch(connection);
					total += batchRows;

					Console.Write(string.Concat("Rows updated = ", total));
					int consoleRow = Console.CursorTop;
					Console.SetCursorPosition(0, consoleRow);

				} while (batchRows > 0);

				Console.WriteLine();

				rows = _personAssignmentCommon.ReadRows(connection, numberOfNotConvertedCommand, null);
				int rowsLeft = rows[0].Field<int>("cnt");
				if (rowsLeft > 0)
				{
					Console.WriteLine("There is still " + rowsLeft + " non converted person assignments in db.");
					return 1;
				}
			}

			Console.WriteLine();

			return 0;
		}

		private int runOneBatch(SqlConnection connection)
		{
			IList<DataRow> rows;
			connection.Open();
			using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
			{
				
				rows = _personAssignmentCommon.ReadRows(connection, _readCommand, transaction);
				_personAssignmentCommon.SetFields(rows);
				updatePersonAssignmentRows(rows, connection, transaction);

				transaction.Commit();
			}
			connection.Close();
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
						.AppendLine("set TheDate = '" + dataRow.Field<DateTime>("TheDate") + "'")
						.AppendLine(", Version = " + dataRow.Field<int>("Version"))
						.AppendLine("where Id='" + dataRow.Field<Guid>("Id") + "'");

					command.CommandText = commandText.ToString();
					command.ExecuteNonQuery();
				}
			}
		}
	}
}