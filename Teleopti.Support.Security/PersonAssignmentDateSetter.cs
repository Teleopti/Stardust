using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Support.Security
{
	public class PersonAssignmentDateSetter : ICommandLineCommand
	{
		private const string checkNumberOfNonConvertedPersonAssignments
			= "select COUNT(*) as cnt from dbo.PersonAssignment where TheDate < '1850-01-01'";

		private readonly string _readCommand = new StringBuilder()
				.AppendLine("select top 100")
				.AppendLine("Pa.Id, DefaultTimeZone, Minimum, TheDate")
				.AppendLine("from dbo.PersonAssignment pa")
				.AppendLine("inner join Person p on pa.Person = p.id")
				.AppendLine("where TheDate = '1800-01-01'")
				.ToString();

		public int Execute(CommandLineArgument commandLineArgument)
		{
			IList<DataRow> rows;
			var dbHelper = new CommonHelper(commandLineArgument.DestinationConnectionString);
			try
			{
				rows = dbHelper.ReadData(checkNumberOfNonConvertedPersonAssignments);
			}
			catch
			{
				//I'm happy, it is an early version
				return 0;
			}

			Console.WriteLine(string.Concat("Found ", rows[0].Field<int>("cnt"), " non converted person assignments"));
			
			int total = 0;
			do
			{
				using (var connection = new SqlConnection(commandLineArgument.DestinationConnectionString))
				{
					total+= runOneBatch(connection);
				}
				Console.Write(string.Concat("Rows updated = ", total));
				Console.SetCursorPosition(0, 1);

			} while (rows.Count > 0);

			Console.WriteLine();

			var numberOfNonConverted = dbHelper.ReadData(checkNumberOfNonConvertedPersonAssignments)[0].Field<int>("cnt");
			if (numberOfNonConverted > 0)
			{
				Console.WriteLine("There is still " + numberOfNonConverted + " non converted assignments in db.");
				return 1;
			}
				
			return 0;
		}

		private int runOneBatch(SqlConnection connection)
		{
			IList<DataRow> rows;
			connection.Open();
			using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
			{
				rows = readRows(connection);
				setFields(rows);
				updatePersonAssignmentRows(rows, connection);

				transaction.Commit();
			}
			connection.Close();
			return rows.Count;
		}

		private static void setFields(IEnumerable<DataRow> rows)
		{
			foreach (var dataRow in rows)
			{
				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(dataRow.Field<string>("DefaultTimeZone"));
				var utcTime = new DateTime(dataRow.Field<DateTime>("Minimum").Ticks, DateTimeKind.Utc);
				var localDate = timeZoneInfo.SafeConvertTimeToUtc(utcTime);
				dataRow["TheDate"] = localDate.Date;
			}
		}

		private IList<DataRow> readRows(SqlConnection connection)
		{
			var command = new SqlCommand(_readCommand, connection);
			IList<DataRow> ret = new List<DataRow>();
			var dataSet = new DataSet();

			using (var sqlDataAdapter = new SqlDataAdapter(command))
			{
				sqlDataAdapter.Fill(dataSet, "Data");
			}

			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				ret.Add(row);
			}

			return ret;
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