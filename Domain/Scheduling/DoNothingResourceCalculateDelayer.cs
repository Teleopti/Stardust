using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DoNothingResourceCalculateDelayer : IResourceCalculateDelayer
	{
		public void CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, bool doIntraIntervalCalculation)
		{
		}
	}
}