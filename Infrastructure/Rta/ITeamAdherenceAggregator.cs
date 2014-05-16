using System;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public interface ITeamAdherenceAggregator
	{
		int Aggregate(Guid teamId);
	}

}