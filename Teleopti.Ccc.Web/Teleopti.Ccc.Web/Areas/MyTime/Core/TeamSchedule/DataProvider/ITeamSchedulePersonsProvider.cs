using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface ITeamSchedulePersonsProvider
	{
		IEnumerable<Guid> RetrievePersonIds(TeamScheduleViewModelData data);
		IEnumerable<IPerson> RetrievePeople(TeamScheduleViewModelData data);
	}
}