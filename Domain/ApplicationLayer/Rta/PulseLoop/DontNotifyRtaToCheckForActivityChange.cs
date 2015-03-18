using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop
{
	public class DontNotifyRtaToCheckForActivityChange : INotifyRtaToCheckForActivityChange
	{
		public void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp, string dataSource)
		{
		}
	}
}