using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface IQueueData : IAnalyticsDataSetup
	{
		IEnumerable<DataRow> Rows { get; }
	}
}