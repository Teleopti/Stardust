using System.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface ITimeZoneData : IAnalyticsDataSetup
	{
		DataTable Table { get; }
		int UtcTimeZoneId { get; }
		int CetTimeZoneId { get; }
	}
}