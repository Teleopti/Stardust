using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IPersonPeriodProvider
	{
		bool HasPersonPeriod(DateOnly date);
	}
}