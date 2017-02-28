using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISchedulingResultStateHolderProvider
	{
		ISchedulingResultStateHolder GiveMeANew();
	}

	public class SchedulingResultStateHolderProvider : ISchedulingResultStateHolderProvider
	{
		public virtual ISchedulingResultStateHolder GiveMeANew()
		{
			return new SchedulingResultStateHolder ();
		}
	}
}
