using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface ITeamScheduleProvider
	{
		IEnumerable<PersonScheduleDayReadModel> TeamSchedule(Guid teamId, DateTime date);
	}
}