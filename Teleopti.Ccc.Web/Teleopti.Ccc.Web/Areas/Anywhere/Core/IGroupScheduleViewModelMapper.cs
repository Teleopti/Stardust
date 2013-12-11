using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGroupScheduleViewModelMapper
	{
		IEnumerable<GroupScheduleShiftViewModel> Map(GroupScheduleData data);
	}
}