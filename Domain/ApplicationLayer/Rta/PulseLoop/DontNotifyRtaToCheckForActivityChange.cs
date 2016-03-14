using System;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop
{
	public class DontNotifyRtaToCheckForActivityChange : INotifyRtaToCheckForActivityChange
	{
		public Task CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			return Task.FromResult(false);
		}
	}
}