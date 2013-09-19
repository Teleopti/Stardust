using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface ITeamScheduleViewModelFactory
	{
		IEnumerable<TeamScheduleShiftViewModel> CreateViewModel(Guid teamId, DateTime date);
	}
}