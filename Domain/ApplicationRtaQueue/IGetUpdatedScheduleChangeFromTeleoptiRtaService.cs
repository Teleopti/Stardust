using System;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
{
	public interface IGetUpdatedScheduleChangeFromTeleoptiRtaService
	{
		void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp);
	}
}