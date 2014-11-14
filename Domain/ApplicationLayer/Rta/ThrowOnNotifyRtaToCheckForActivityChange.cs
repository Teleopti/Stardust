using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ThrowOnNotifyRtaToCheckForActivityChange : INotifyRtaToCheckForActivityChange
	{
		public void CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			throw new NotImplementedException();
		}
	}
}