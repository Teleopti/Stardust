using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleViewModelFactory
	{
		GroupScheduleViewModel CreateViewModel(IDictionary<PersonFinderField, string> criteriaDictionary, DateOnly dateInUserTimeZone, int pageSize, int currentPageIndex);
	}
}