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
		AgentScheduleViewModelReworked Map(IPersonScheduleDayReadModel scheduleReadModel);
		IEnumerable<AgentScheduleViewModelReworked> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels);

	}
}
