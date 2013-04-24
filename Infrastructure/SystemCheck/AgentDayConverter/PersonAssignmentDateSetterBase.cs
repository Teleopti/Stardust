using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public abstract class PersonAssignmentDateSetterBase : IPersonAssignmentConverter
	{
		protected abstract string NumberOfNotConvertedCommand { get; }
		protected abstract string ReadCommand { get; }
		protected abstract string UpdateAssignmentDate { get; }

		public void Execute(SqlConnectionStringBuilder connectionStringBuilder)
		{
			var connectionString = connectionStringBuilder.ToString();

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				var rows = readRows(connection, ReadCommand);
				setFields(rows);
				updatePersonAssignmentRows(rows, connection);

				checkAllConverted(connection);
			}
		}

		private void checkAllConverted(SqlConnection sqlConnection)
		{
			if ((int) new SqlCommand(NumberOfNotConvertedCommand, sqlConnection).ExecuteScalar() > 0)
			{
				throw new NotSupportedException("Something went wrong. There is still unconverted schedules in the database!");
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

		private static void setFields(IEnumerable<DataRow> rows)
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

		private IEnumerable<DataRow> readRows(SqlConnection connection, string readCommand)
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
	}
}