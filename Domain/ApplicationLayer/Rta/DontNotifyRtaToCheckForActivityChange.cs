using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class DontNotifyRtaToCheckForActivityChange : INotifyRtaToCheckForActivityChange
	{
		public void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
		}
	}
}