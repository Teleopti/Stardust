using System.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public interface IDateData : IAnalyticsDataSetup
	{
		DataTable Table { get; }
	}
}