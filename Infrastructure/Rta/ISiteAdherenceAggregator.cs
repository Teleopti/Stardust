using System;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public interface ISiteAdherenceAggregator
	{
		int Aggregate(Guid siteId);
	}
}