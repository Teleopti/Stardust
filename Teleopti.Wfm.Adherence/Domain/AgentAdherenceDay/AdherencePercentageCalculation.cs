using System;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public static class AdherencePercentageCalculation
	{
		public static int? Calculate(int? secondsInAdherence, int? secondsOutOfAdherence)
		{
			var secondsIn = (double) secondsInAdherence.GetValueOrDefault();
			var secondsOut = (double) secondsOutOfAdherence.GetValueOrDefault();
			var secondsWorkTime = secondsIn + secondsOut;
			if (secondsWorkTime.Equals(0))
				return null;
			return Convert.ToInt32(secondsIn / secondsWorkTime * 100);
		}
	}
}