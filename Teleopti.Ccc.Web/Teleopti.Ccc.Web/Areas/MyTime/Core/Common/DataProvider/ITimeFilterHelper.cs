using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ITimeFilterHelper
	{
		TimeFilterInfo GetFilter(DateOnly selectedDate, string filterStartTimes, string filterEndTimes, bool isDayOff);
	}
}