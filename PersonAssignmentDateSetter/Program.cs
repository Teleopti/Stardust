﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonAssignmentDateSetter
{
	class Program
	{
		static void Main(string[] args)
		{
			runPersonAssignment(args);
			Console.WriteLine();
			runAuditPersonAssignment(args);
			Console.WriteLine();
			Console.WriteLine("Ready, press any key");
			Console.ReadKey();
		}

		private static void runPersonAssignment(string[] args)
		{
			CommonHelper dbHelper = new CommonHelper(args[0]);

			IList<DataRow> rows = dbHelper.ReadData("select COUNT(*) as cnt from dbo.PersonAssignment where TheDate = '1800-01-01'");
			Console.WriteLine("Updating " + dbHelper.SqlConnectionStringBuilder().InitialCatalog + " found " + rows[0].Field<int>("cnt") + " person assignments");
			StringBuilder commandString = new StringBuilder();
			commandString.AppendLine("select top 100");
			commandString.AppendLine("Pa.Id, DefaultTimeZone, Minimum, TheDate");
			commandString.AppendLine("from dbo.PersonAssignment pa");
			commandString.AppendLine("inner join Person p on pa.Person = p.id");
			commandString.AppendLine("Where TheDate = '1800-01-01'");

			int total = 0;
			do
			{
				rows = dbHelper.ReadData(commandString.ToString());
				total += rows.Count;
				foreach (var dataRow in rows)
				{
					TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(dataRow.Field<string>("DefaultTimeZone"));
					DateTime utcTime = new DateTime(dataRow.Field<DateTime>("Minimum").Ticks, DateTimeKind.Utc);
					DateTime localDate = timeZoneInfo.SafeConvertTimeToUtc(utcTime);
					dataRow["TheDate"] = localDate.Date;
				}
				dbHelper.UpdatePersonAssignmentRows(rows);

				Console.SetCursorPosition(0, 1);
				Console.Write("Rows updated = " + total);

			} while (rows.Count > 0);

			Console.WriteLine();
		}

		private static void runAuditPersonAssignment(string[] args)
		{
			CommonHelper dbHelper = new CommonHelper(args[0]);

			IList<DataRow> rows = dbHelper.ReadData("select COUNT(*) as cnt from [Auditing].PersonAssignment_AUD where TheDate = '1800-01-01'");
			Console.WriteLine("Updating " + dbHelper.SqlConnectionStringBuilder().InitialCatalog + " found " + rows[0].Field<int>("cnt") + " person assignments");
			StringBuilder commandString = new StringBuilder();
			commandString.AppendLine("select top 100");
			commandString.AppendLine("Pa.Id, DefaultTimeZone, Minimum, TheDate");
			commandString.AppendLine("from [Auditing].PersonAssignment_AUD pa");
			commandString.AppendLine("inner join Person p on pa.Person = p.id");
			commandString.AppendLine("Where TheDate = '1800-01-01'");

			int total = 0;
			do
			{
				rows = dbHelper.ReadData(commandString.ToString());
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

			Console.WriteLine();
		}
	}
}
