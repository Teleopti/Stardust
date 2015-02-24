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

		public Percent ForDay(AdherencePercentageReadModel model)
		{
			var secondsInAdherence = Convert.ToDouble(model.TimeInAdherence.TotalSeconds);
			var secondsOutOfAdherence = Convert.ToDouble(model.TimeOutOfAdherence.TotalSeconds);

			if (!model.ShiftHasEnded)
			{
				var isLastInAdherence = model.IsLastTimeInAdherence ?? false;
				var lastTimestamp = model.LastTimestamp ?? DateTime.MinValue;
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
