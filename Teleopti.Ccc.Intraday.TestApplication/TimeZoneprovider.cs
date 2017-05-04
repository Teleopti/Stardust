using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Intraday.TestApplication.Infrastructure;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class TimeZoneprovider
	{
		private readonly string _connectionString;

		public TimeZoneprovider(string connectionString)
		{
			_connectionString = connectionString;
		}

		public TimeZoneInterval Provide(string timeZoneCode)
		{
			var dbCommand = new DatabaseCommand(CommandType.StoredProcedure, "mart.web_intraday_simulator_get_timezone", _connectionString);

			var parameterList = new[]
			{
				new SqlParameter("@time_zone_code", timeZoneCode)
			};

			DataSet resultSet = dbCommand.ExecuteDataSet(parameterList);

			if (resultSet == null || resultSet.Tables.Count != 1)
			{
				throw new ApplicationException("Could not return Analytics default timezone and interval length.");
			}

			var table = resultSet.Tables[0];

			return new TimeZoneInterval
			{
				TimeZoneId = (string)table.Rows[0]["TimeZoneId"],
				IntervalLength = Convert.ToInt32(table.Rows[0]["IntervalLength"])
			};
		}
	}
}