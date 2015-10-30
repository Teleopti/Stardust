using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGroupScheduleViewModelFactory
	{
		IEnumerable<GroupScheduleShiftViewModel> CreateViewModel(Guid teamId, DateTime dateInUserTimeZone);

		IEnumerable<GroupScheduleShiftViewModel> LoadSchedulesWithPaging(Guid groupId, DateTime dateInUserTimeZone,
			int pageSize, int currentPageIndex, out int totalPage);
	}
}
