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
		private readonly int _answeredCalls;
		private readonly int _transferedCalls;

		public FactAgentQueue(int dateId, int queueId, int acdLoginId, decimal talkTime, decimal afterCallWorkTime, int answeredCalls, int transferedCalls)
		{
			_dateId = dateId;
			_queueId = queueId;
			_acdLoginId = acdLoginId;
			_talkTime = talkTime;
			_afterCallWorkTime = afterCallWorkTime;
			_answeredCalls = answeredCalls;
			_transferedCalls = transferedCalls;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = fact_agent_queue.CreateTable())
			{
				table.AddFactAgentQueue(_dateId, 1, _queueId, _acdLoginId, _dateId, _talkTime, _afterCallWorkTime,
																 _answeredCalls, _transferedCalls);
				Bulk.Insert(connection, table);
			}
		}
	}
}