using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IStageRequest
	{
		IEnumerable<DataRow> Rows { get; }
	}
}