using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Intraday.TestApplication.Infrastructure;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class ForecastProvider : IForecastProvider
	{
		private readonly string _connectionString;

		public ForecastProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		public IList<ForecastInterval> Provide(int workloadId, int intervalId, DateTime date)
		{
			var forecastIntervals = new List<ForecastInterval>();
			var dbCommand = new DatabaseCommand(CommandType.StoredProcedure, "mart.web_intraday_simulator_get_forecast", _connectionString);

			var parameterList = new[]
												{
													 new SqlParameter("@workload_id", workloadId),	
													 new SqlParameter("@today", date.Date),
													 new SqlParameter("@interval_id", intervalId)
												};

			DataSet resultSet = dbCommand.ExecuteDataSet(parameterList);

			if (resultSet == null || resultSet.Tables.Count == 0)
			{
				return forecastIntervals;
			}

			var table = resultSet.Tables[0];

			foreach (DataRow row in table.Rows)
			{
				var interval = new ForecastInterval
				{
					DateId = (int) row["date_id"],
					IntervalId = Convert.ToInt16(row["interval_id"]),
					Calls = Convert.ToDouble(row["forecasted_calls"]),					
                    TalkTime = Convert.ToDouble(row["forecasted_talk_time_s"]),
                    AfterTalkTime = Convert.ToDouble(row["forecasted_after_call_work_s"]),
                    HandleTime = Convert.ToDouble(row["forecasted_handling_time_s"])
                };

				forecastIntervals.Add(interval);
			}

			return forecastIntervals;
		}
	}
}