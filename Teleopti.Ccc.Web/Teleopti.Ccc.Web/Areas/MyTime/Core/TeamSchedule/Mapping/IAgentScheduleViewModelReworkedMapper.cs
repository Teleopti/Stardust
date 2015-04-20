using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface IAgentScheduleViewModelReworkedMapper
	{
		AgentScheduleViewModelReworked Map(PersonSchedule personSchedule);
		IEnumerable<AgentScheduleViewModelReworked> Map(IEnumerable<PersonSchedule> personSchedules);

	}
}
