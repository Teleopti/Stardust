using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface ITeamSchedulePersonsProvider
	{
		IEnumerable<Guid> RetrievePersons(TeamScheduleViewModelData data);
	}
}