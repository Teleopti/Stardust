using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IGetUpdatedScheduleChangeFromTeleoptiRtaService
	{
		void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp);
	}
}