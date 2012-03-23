using System.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface IBusinessUnitData
	{
		DataTable Table { get; }
		int BusinessUnitId { get; }
	}
}