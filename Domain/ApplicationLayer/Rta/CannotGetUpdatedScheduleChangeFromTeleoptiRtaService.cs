using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public class CannotGetUpdatedScheduleChangeFromTeleoptiRtaService : IGetUpdatedScheduleChangeFromTeleoptiRtaService
	{
		public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			throw new NotImplementedException();
		}
	}
}