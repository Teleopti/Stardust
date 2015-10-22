using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatPlanner
	{
		void CreateSeatPlansForPeriod(ISeatMapLocation rootSeatMapLocation, ICollection<ITeam> teams, DateOnlyPeriod period);
		void CreateSeatPlansForPeriod(IEnumerable<ISeat> seats, List<Guid> personIds, DateOnlyPeriod period);
	}
}