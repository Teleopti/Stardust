using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class RestrictionsAbleToBeScheduled
	{
		public bool Execute(IVirtualSchedulePeriod schedulePeriod)
		{
			return false;
		}
	}
}