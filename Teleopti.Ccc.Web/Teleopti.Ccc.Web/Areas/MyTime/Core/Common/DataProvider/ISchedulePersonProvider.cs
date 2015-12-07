using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ISchedulePersonProvider
	{
		IEnumerable<IPerson> GetPermittedPersonsForTeam(DateOnly date, Guid id, string function);
		IEnumerable<IPerson> GetPermittedPersonsForGroup(DateOnly date, Guid id, string function);
		IEnumerable<IPerson> GetPermittedPeople(GroupScheduleInput input, string function);
	}
}