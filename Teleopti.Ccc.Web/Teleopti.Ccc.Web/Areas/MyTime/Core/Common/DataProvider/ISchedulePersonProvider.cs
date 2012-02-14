using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ISchedulePersonProvider
	{
		IEnumerable<IPerson> GetPermittedPersonsForTeam(DateOnly date, Guid id);
	}
}