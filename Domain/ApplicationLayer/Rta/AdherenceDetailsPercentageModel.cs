using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceDetailsPercentageModel
	{
		public int AdherencePercent { get; set; }
		public string Name { get; set; }
		public string StartTime { get; set; }
		public string ActualStartTime { get; set; }
	}
}