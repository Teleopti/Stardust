using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherencePercentageCalculator
	{
		public void Calculate(AdherencePercentageReadModel model)
		{
			model.TimeInAdherence = TimeSpan.Zero;
			model.TimeOutOfAdherence = TimeSpan.Zero;
			AdherencePercentageState previous = null;
			foreach (var current in model.Saga.OrderBy(x => x.Timestamp))
			{
				if (previous != null)
				{
					var timeDifferenceBetweenCurrentAndPrevious = current.Timestamp - previous.Timestamp;
					switch (previous.Adherence)
					{
						case Adherence.Out:
							model.TimeOutOfAdherence += timeDifferenceBetweenCurrentAndPrevious;
							break;
						case Adherence.In:
							model.TimeInAdherence += timeDifferenceBetweenCurrentAndPrevious;
							break;
					}
				}
				if (current.ShiftEnded)
					break;
				previous = current;
			}
		}
	}
}