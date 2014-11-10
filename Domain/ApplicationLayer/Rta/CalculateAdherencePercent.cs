using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class CalculateAdherencePercent
	{
		private readonly INow _now;

		public CalculateAdherencePercent(INow now)
		{
			_now = now;
		}

		public Percent ForDay(AdherencePercentageReadModel data)
		{
			var secondsInAdherence = Convert.ToDouble(data.TimeInAdherence.TotalSeconds);
			var secondsOutOfAdherence = Convert.ToDouble(data.TimeOutOfAdherence.TotalSeconds);

			if (!data.ShiftHasEnded)
			{
				var isLastInAdherence = data.IsLastTimeInAdherence ?? false;
				var lastTimestamp = data.LastTimestamp ?? DateTime.MinValue;
				var secondsFromLastUpdate = _now.UtcDateTime().Subtract(lastTimestamp).TotalSeconds;
				if (isLastInAdherence)
					secondsInAdherence += secondsFromLastUpdate;
				else
					secondsOutOfAdherence += secondsFromLastUpdate;
			}

			var total = secondsInAdherence + secondsOutOfAdherence;

			return new Percent(secondsInAdherence / total);
		}

	}

}
