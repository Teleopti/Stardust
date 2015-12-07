using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public interface ITeamScheduleViewModelFactory
	{
		IEnumerable<GroupScheduleShiftViewModel> CreateViewModel(Guid groupId, DateTime dateInUserTimeZone);
		GroupScheduleViewModel CreateViewModel(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone);
		GroupScheduleViewModel CreateViewModel(GroupScheduleInput input);
	}
}