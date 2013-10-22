using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public interface IPersonData
	{
		IEnumerable<DataRow> Rows { get; }
		int PersonId { get; }
	}
}