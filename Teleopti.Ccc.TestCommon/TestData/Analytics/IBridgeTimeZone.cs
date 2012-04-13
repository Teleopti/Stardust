using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IBridgeTimeZone
	{
		IEnumerable<DataRow> Rows { get; }
	}
}