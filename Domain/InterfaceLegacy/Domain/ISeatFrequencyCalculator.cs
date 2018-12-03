using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatFrequencyCalculator
	{
		Dictionary<Guid, List<ISeatOccupancyFrequency>> GetSeatPopulationFrequency (DateOnlyPeriod period, List<IPerson> people);
	}
}