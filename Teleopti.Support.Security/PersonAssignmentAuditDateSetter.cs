using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Support.Security
{
	public class PersonAssignmentAuditDateSetter
	{
		private const string checkNumberOfNonConvertedAuditPersonAssignments
			= "select COUNT(*) as cnt from [Auditing].PersonAssignment_AUD where TheDate < '1850-01-01'";

		private static void runAuditPersonAssignment(string[] args)
		{
			CommonHelper dbHelper = new CommonHelper(args[0]);

			IList<DataRow> rows = dbHelper.ReadData(checkNumberOfNonConvertedAuditPersonAssignments);
			Console.WriteLine("Updating " + dbHelper.SqlConnectionStringBuilder().InitialCatalog + " found " + rows[0].Field<int>("cnt") + " audit person assignments");
			var commandString = new StringBuilder()
				.AppendLine("select top 100")
				.AppendLine("Pa.Id, DefaultTimeZone, Minimum, TheDate")
				.AppendLine("from [Auditing].PersonAssignment_AUD pa")
				.AppendLine("inner join Person p on pa.Person = p.id")
				.AppendLine("Where TheDate = '1800-01-01'").ToString();

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
				dbHelper.UpdateAuditPersonAssignmentRows(rows);



				Console.SetCursorPosition(0, 1);
				Console.Write("Rows updated = " + total);

			} while (rows.Count > 0);

			var numberOfNonConverted = dbHelper.ReadData(checkNumberOfNonConvertedAuditPersonAssignments)[0].Field<int>("cnt");
			if (numberOfNonConverted > 0)
				throw new Exception("There is still " + numberOfNonConverted + " non converted audit assignments in db.");

			Console.WriteLine();
		}
	}
}