using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
	public class PersonAssignmentCommon : IPersonAssignmentCommon
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public IList<DataRow> ReadRows(SqlConnection connection, string readCommand, SqlTransaction transaction)
		{
			IList<DataRow> ret;
			using (var command = new SqlCommand(readCommand, connection))
			{
				ret = new List<DataRow>();
				using (var dataSet = new DataSet())
				{
					dataSet.Locale = CultureInfo.CurrentUICulture;
					using (var sqlDataAdapter = new SqlDataAdapter(command))
					{
						sqlDataAdapter.SelectCommand.Transaction = transaction;
						sqlDataAdapter.Fill(dataSet, "Data");
					}

					foreach (DataRow row in dataSet.Tables[0].Rows)
					{
						ret.Add(row);
					}
				}
			}

			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetFields(IEnumerable<DataRow> rows)
		{
			foreach (var dataRow in rows)
			{
				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(dataRow.Field<string>("DefaultTimeZone"));
				var utcTime = new DateTime(dataRow.Field<DateTime>("Minimum").Ticks, DateTimeKind.Utc);
				var localDate = timeZoneInfo.SafeConvertTimeToUtc(utcTime);
				dataRow["TheDate"] = localDate.Date;
				var version = dataRow.Field<int>("Version");
				dataRow["Version"] = version + 1;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool TheDateFieldExists(SqlConnection connection, string numberOfNotConvertedCommand)
		{
			try
			{
				ReadRows(connection, numberOfNotConvertedCommand, null);
			}
			catch
			{
				return false;
			}

			return true;
		}
	}
}