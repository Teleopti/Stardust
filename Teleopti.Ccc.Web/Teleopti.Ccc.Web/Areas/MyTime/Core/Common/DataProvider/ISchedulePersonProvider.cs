using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ISchedulePersonProvider
	{
		IEnumerable<IPerson> GetPermittedPersonsForGroup(DateOnly date, Guid id, string function);
		IEnumerable<IPerson> GetPermittedPeople(GroupScheduleInput input, string function);
	}
}