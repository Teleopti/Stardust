using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace PersonAssignmentDateSetter
{
	class Program
	{
		private const string checkNumberOfNonConvertedPersonAssignments
			= "select COUNT(*) as cnt from PersonAssignment where TheDate < '1850-01-01'";

		static void Main(string[] args)
		{

			CommonHelper dbHelper = new CommonHelper(args[0]);

			IList<DataRow> rows = dbHelper.ReadData(checkNumberOfNonConvertedPersonAssignments);
			Console.WriteLine("Updating " + dbHelper.SqlConnectionStringBuilder().InitialCatalog + " found " + rows[0].Field<int>("cnt") + " person assignments");
			var commandString = new StringBuilder()
				.AppendLine("select top 100")
				.AppendLine("Pa.Id, DefaultTimeZone, Minimum, TheDate")
				.AppendLine("from PersonAssignment pa")
				.AppendLine("inner join Person p on pa.Person = p.id")
				.AppendLine("Where TheDate = '1800-01-01'")
				.ToString();
			
			int total = 0;
			do
			{
				rows = dbHelper.ReadData(commandString);
				total += rows.Count;
				foreach (var dataRow in rows)
				{
					TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(dataRow.Field<string>("DefaultTimeZone"));
					DateTime utcTime = new DateTime(dataRow.Field<DateTime>("Minimum").Ticks, DateTimeKind.Utc);
					DateTime localDate = timeZoneInfo.SafeConvertTimeToUtc(utcTime);
					dataRow["TheDate"] = localDate.Date;
				}
				dbHelper.UpdateRows(rows);

				Console.Clear();

				Console.SetCursorPosition(0,1);
				Console.Write("Rows updated = " + total);
				
			} while (rows.Count > 0);

			var numberOfNonConverted = dbHelper.ReadData(checkNumberOfNonConvertedPersonAssignments)[0].Field<int>("cnt");
			if (numberOfNonConverted> 0)
				throw new Exception("There is still " + numberOfNonConverted + " non converted assignments in db.");

			Console.WriteLine();
			Console.WriteLine("Ready, press any key");
			Console.ReadKey();
		}
	}
}
