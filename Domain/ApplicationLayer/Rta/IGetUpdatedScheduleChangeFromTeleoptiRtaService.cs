using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public interface IGetUpdatedScheduleChangeFromTeleoptiRtaService
	{
		void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp);
	}
}