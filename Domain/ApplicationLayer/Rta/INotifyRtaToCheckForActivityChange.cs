using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface INotifyRtaToCheckForActivityChange
	{
		void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp);
	}
}