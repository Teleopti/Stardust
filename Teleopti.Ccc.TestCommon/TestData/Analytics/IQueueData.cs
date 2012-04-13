using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IQueueData : IAnalyticsDataSetup
	{
		IEnumerable<DataRow> Rows { get; }
	}
}