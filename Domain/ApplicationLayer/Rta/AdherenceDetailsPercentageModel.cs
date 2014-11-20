using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceDetailsPercentageModel
	{
		public int AdherencePercent { get; set; }
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime ActualStartTime { get; set; }
	}
}