using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop
{
	public interface INotifyRtaToCheckForActivityChange
	{
		void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp, string dataSource);
	}
}