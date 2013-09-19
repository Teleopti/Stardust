using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface ITeamScheduleViewModelMapper
	{
		IEnumerable<Shift> Map(TeamScheduleData data);
	}
}