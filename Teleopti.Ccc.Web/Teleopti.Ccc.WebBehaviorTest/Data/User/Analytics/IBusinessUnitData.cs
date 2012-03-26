using System.Collections.Generic;
using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface IBusinessUnitData
	{
		IEnumerable<DataRow> Rows { get; }
		int BusinessUnitId { get; }
	}
}