using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGroupScheduleViewModelFactory
	{
		IEnumerable<GroupScheduleShiftViewModel> CreateViewModel(Guid teamId, DateTime dateInUserTimeZone);
	}
}
