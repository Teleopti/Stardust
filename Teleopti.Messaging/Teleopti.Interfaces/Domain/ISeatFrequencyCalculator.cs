using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatFrequencyCalculator
	{
		Dictionary<Guid, List<ISeatOccupancyFrequency>> GetSeatPopulationFrequency (DateOnlyPeriod period, List<IPerson> people);
	}
}