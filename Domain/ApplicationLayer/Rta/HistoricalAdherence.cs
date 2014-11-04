using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IHistoricalAdherence
	{
		Percent ForDay(AdherencePercentageReadModel data);
	}

	public class HistoricalAdherence : IHistoricalAdherence
	{
		private readonly INow _now;

		public HistoricalAdherence(INow now)
		{
			_now = now;
		}

		public Percent ForDay(AdherencePercentageReadModel data)
		{
			var secondsInAdherence =Convert.ToDouble(data.TimeInAdherence.TotalSeconds);
			var secondsOutOfAdherence = Convert.ToDouble(data.TimeOutOfAdherence.TotalSeconds);
			var fromLatestStateChange = numberOfSecondsForLastState(data.LastTimestamp, data.ShiftEnd);

			if (data.IsLastTimeInAdherence)
			{
				secondsInAdherence += fromLatestStateChange;
			}
			else
			{
				secondsOutOfAdherence += fromLatestStateChange;
			}

			var total = secondsInAdherence + secondsOutOfAdherence;

			return new Percent(secondsInAdherence / total);
		}

		private double numberOfSecondsForLastState(DateTime lastTimeStamp, DateTime? shiftEndDateTime)
		{
			if (shiftEndDateTime!=null && _now.UtcDateTime() > shiftEndDateTime)
			{
				return ((DateTime)shiftEndDateTime).Subtract(lastTimeStamp).TotalSeconds;
			}
			return _now.UtcDateTime().Subtract(lastTimeStamp).TotalSeconds;				
		}
	}

}
