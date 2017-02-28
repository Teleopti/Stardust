using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DoNothingResourceCalculateDelayer : IResourceCalculateDelayer
	{
		public void CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod, bool doIntraIntervalCalculation)
		{
		}

		public void Pause()
		{
		}

		public void Resume()
		{
		}
	}
}