using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface IBridgeTimeZone
	{
		IEnumerable<DataRow> Rows { get; }
	}
}