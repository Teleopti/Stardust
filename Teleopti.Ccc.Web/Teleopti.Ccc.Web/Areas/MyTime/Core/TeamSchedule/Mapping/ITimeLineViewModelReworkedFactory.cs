using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface ITimeLineViewModelReworkedFactory
	{
		IEnumerable<TimeLineViewModelReworked> CreateTimeLineHours(DateTimePeriod timeLinePeriod);
	}
}