using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatFrequencyCalculator
	{
		Dictionary<Guid, List<ISeatOccupancyFrequency>> GetSeatPopulationFrequency (DateOnlyPeriod period, List<IPerson> people);
	}
}