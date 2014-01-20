using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FactAgentQueue : IAnalyticsDataSetup
	{
		private readonly int _dateId;
		private readonly int _queueId;
		private readonly int _acdLoginId;
		private readonly decimal _talkTime;
		private readonly decimal _afterCallWorkTime;
		private readonly decimal _answeredCallsWithin;
		private readonly int _answeredCalls;
		private readonly int _transferedCalls;
		private readonly int _dataSourceId;

		public FactAgentQueue(int dateId, int queueId, int acdLoginId, decimal talkTime, decimal afterCallWorkTime, decimal answeredCallsWithin, int answeredCalls, int transferedCalls, int dataSourceId)
		{
			_dateId = dateId;
			_queueId = queueId;
			_acdLoginId = acdLoginId;
			_talkTime = talkTime;
			_afterCallWorkTime = afterCallWorkTime;
			_answeredCallsWithin = answeredCallsWithin;
			_answeredCalls = answeredCalls;
			_transferedCalls = transferedCalls;
			_dataSourceId = dataSourceId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = fact_agent_queue.CreateTable())
			{
				table.AddFactAgentQueue(_dateId, 1, _queueId, _acdLoginId, _dateId, _talkTime, _afterCallWorkTime,
				                        _answeredCallsWithin, _answeredCalls, _transferedCalls, _dataSourceId);
				Bulk.Insert(connection, table);
			}
		}
	}
}