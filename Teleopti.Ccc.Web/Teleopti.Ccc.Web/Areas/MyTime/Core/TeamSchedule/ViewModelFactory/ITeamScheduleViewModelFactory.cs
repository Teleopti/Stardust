using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public interface ITeamScheduleViewModelFactory
	{
		TeamScheduleViewModel CreateViewModel(DateOnly date, Guid id);
		IEnumerable<ISelectOption> CreateTeamOrGroupOptionsViewModel(DateOnly date);
	}
}