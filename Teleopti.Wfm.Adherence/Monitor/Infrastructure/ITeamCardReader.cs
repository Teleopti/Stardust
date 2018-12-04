using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public interface ITeamCardReader
	{
		IEnumerable<TeamCardModel> Read();
		IEnumerable<TeamCardModel> Read(IEnumerable<Guid> skillIds);

		IEnumerable<TeamCardModel> Read(Guid siteId);
		IEnumerable<TeamCardModel> Read(Guid siteId, IEnumerable<Guid> skillIds);
	}
}