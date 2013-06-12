using System;

namespace Teleopti.Ccc.Domain.ApplicationRtaQueue
{
	public class CannotGetUpdatedScheduleChangeFromTeleoptiRtaService : IGetUpdatedScheduleChangeFromTeleoptiRtaService
	{
		public void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			throw new NotImplementedException();
		}
	}
}