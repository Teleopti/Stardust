using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface ITimeZoneData : IAnalyticsDataSetup
	{
		IEnumerable<DataRow> Rows { get; }
		int UtcTimeZoneId { get; }
		int CetTimeZoneId { get; }
	}
}