using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IOvertimeRequestOpenPeriodMerger
	{
		List<OvertimeRequestSkillTypeFlatOpenPeriod> GetMergedOvertimeRequestOpenPeriods(IEnumerable<IOvertimeRequestOpenPeriod> overtimeRequestOpenPeriods,
			IPermissionInformation permissionInformation, DateOnlyPeriod period);
	}
}