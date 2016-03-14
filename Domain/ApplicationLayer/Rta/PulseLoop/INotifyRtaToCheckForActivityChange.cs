using System;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop
{
	public interface INotifyRtaToCheckForActivityChange
	{
		Task CheckForActivityChange(Guid personId, Guid businessUnitId, DateTime timestamp);
	}
}