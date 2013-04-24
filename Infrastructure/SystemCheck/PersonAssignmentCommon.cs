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
		public IList<DataRow> ReadRows(SqlConnection connection, string readCommand)
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

		public void SetFields(IEnumerable<DataRow> rows)
		{
			foreach (var dataRow in rows)
			{
				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(dataRow.Field<string>("DefaultTimeZone"));
				var utcTime = new DateTime(dataRow.Field<DateTime>("Minimum").Ticks, DateTimeKind.Utc);
				var localDate = timeZoneInfo.SafeConvertTimeToUtc(utcTime);
				dataRow["TheDate"] = String.Format("{0:s}", localDate.Date);
				var version = dataRow.Field<int>("Version");
				dataRow["Version"] = version + 1;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public bool TheDateFieldExists(SqlConnection connection, string numberOfNotConvertedCommand)
		{
			try
			{
				ReadRows(connection, numberOfNotConvertedCommand);
			}
			catch
			{
				return false;
			}

			return true;
		}
	}
}