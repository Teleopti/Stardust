using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IActivityData
	{
		IEnumerable<DataRow> Rows { get; }
	}
}