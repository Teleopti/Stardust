using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface INumberOfAgentsInTeamReader
	{
		IDictionary<Guid, int> Read(IEnumerable<Guid> teams);
		IDictionary<Guid, int> Read(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);
	}
}