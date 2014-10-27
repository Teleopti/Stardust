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
		public Percent ForDay(AdherencePercentageReadModel data)
		{
			var minIn =Convert.ToDouble(data.MinutesInAdherence);
			var total = data.MinutesInAdherence + data.MinutesOutOfAdherence;
			return new Percent(minIn / total);
		}
	}
}
