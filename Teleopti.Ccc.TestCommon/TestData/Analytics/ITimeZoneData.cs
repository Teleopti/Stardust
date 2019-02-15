using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface ITimeZoneData : IAnalyticsDataSetup
	{
		IEnumerable<DataRow> Rows { get; }
	}
}