using System.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface IWorkloadData : IAnalyticsDataSetup
	{
		DataTable Table { get; }
	}
}