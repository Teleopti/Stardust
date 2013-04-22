using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Support.Security
{
	public class PersonAssignmentCommon
	{
		public IList<DataRow> ReadRows(SqlConnection connection, string readCommand, SqlTransaction transaction)
		{
			var command = new SqlCommand(readCommand, connection);
			IList<DataRow> ret = new List<DataRow>();
			var dataSet = new DataSet();

			using (var sqlDataAdapter = new SqlDataAdapter(command))
			{
				sqlDataAdapter.SelectCommand.Transaction = transaction;
				sqlDataAdapter.Fill(dataSet, "Data");
			}

			foreach (DataRow row in dataSet.Tables[0].Rows)
			{
				ret.Add(row);
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
				dataRow["TheDate"] = localDate.Date;
			}
		}

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