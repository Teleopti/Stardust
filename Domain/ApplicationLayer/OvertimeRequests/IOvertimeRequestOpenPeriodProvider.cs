using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestOpenPeriodProvider
	{
		IList<OvertimeRequestSkillTypeFlatOpenPeriod> GetOvertimeRequestOpenPeriods(IPerson person, DateTimePeriod period);
	}
}
