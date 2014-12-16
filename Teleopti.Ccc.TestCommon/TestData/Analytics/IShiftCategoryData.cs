using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IShiftCategoryData
	{
		IEnumerable<DataRow> Rows { get; }
	}
}