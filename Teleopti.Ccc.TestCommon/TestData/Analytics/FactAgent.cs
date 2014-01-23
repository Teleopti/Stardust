using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FactAgent : IAnalyticsDataSetup
	{
		private readonly int _dateId;
		private readonly int _intervalId;
		private readonly int _acdLoginId;
		private readonly int _localDateId;
		private readonly int _localIntervalId;
		private readonly int _readyTimeS;
		private readonly int _loggedInTimeS;
		private readonly int _notReadyTimeS;
		private readonly int _idleTimeS;
		private readonly int _directOutboundCalls;
		private readonly int _directOutboundTalkTimeS;
		private readonly int _directIncomingCalls;
		private readonly int _directIncomingCallsTalkTimeS;
		private readonly int _adminTimeS;

		public FactAgent(int dateId, int intervalId, int acdLoginId, int localDateId, int localIntervalId, int readyTimeS, int loggedInTimeS, int notReadyTimeS, int idleTimeS, int directOutboundCalls, int directOutboundTalkTimeS, int directIncomingCalls, int directIncomingCallsTalkTimeS, int adminTimeS)
		{
			_dateId = dateId;
			_intervalId = intervalId;
			_acdLoginId = acdLoginId;
			_localDateId = localDateId;
			_localIntervalId = localIntervalId;
			_readyTimeS = readyTimeS;
			_loggedInTimeS = loggedInTimeS;
			_notReadyTimeS = notReadyTimeS;
			_idleTimeS = idleTimeS;
			_directOutboundCalls = directOutboundCalls;
			_directOutboundTalkTimeS = directOutboundTalkTimeS;
			_directIncomingCalls = directIncomingCalls;
			_directIncomingCallsTalkTimeS = directIncomingCallsTalkTimeS;
			_adminTimeS = adminTimeS;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = fact_agent.CreateTable())
			{
				table.AddFactAgent(_dateId, _intervalId, _acdLoginId, _localDateId, _localIntervalId, _readyTimeS, _loggedInTimeS,
					_notReadyTimeS, _idleTimeS,
					_directOutboundCalls, _directOutboundTalkTimeS, _directIncomingCalls, _directIncomingCallsTalkTimeS, _adminTimeS);
				Bulk.Insert(connection, table);
			}
		}
	}
}