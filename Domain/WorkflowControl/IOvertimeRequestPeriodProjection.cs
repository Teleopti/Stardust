using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IOvertimeRequestPeriodProjection
	{
		IList<OvertimeRequestSkillTypeFlatOpenPeriod> GetProjectedOvertimeRequestsOpenPeriods(DateOnlyPeriod requestPeriod);
	}
}