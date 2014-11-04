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
			var minIn =Convert.ToDouble(data.TimeInAdherence.TotalSeconds);
			var minOut = Convert.ToDouble(data.TimeOutOfAdherence.TotalSeconds);
			var fromLatestStateChange = numberOfMinutesForLastState(data.LastTimestamp, data.ShiftEnd);

			if (data.IsLastTimeInAdherence)
			{
				minIn += fromLatestStateChange;
			}
			else
			{
				minOut += fromLatestStateChange;
			}

			var total = minIn + minOut;

			return new Percent(minIn / total);
		}

		private double numberOfMinutesForLastState(DateTime lastTimeStamp, DateTime? shiftEndDateTime)
		{
			if (shiftEndDateTime!=null && _now.UtcDateTime() > shiftEndDateTime)
			{
				return ((DateTime)shiftEndDateTime).Subtract(lastTimeStamp).TotalMinutes;
			}
			return _now.UtcDateTime().Subtract(lastTimeStamp).TotalMinutes;				
		}
	}

}
