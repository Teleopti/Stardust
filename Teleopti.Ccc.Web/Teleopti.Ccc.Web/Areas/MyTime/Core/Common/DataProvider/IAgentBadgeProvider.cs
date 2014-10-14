using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAgentBadgeProvider
	{
		IEnumerable<IPerson> GetPermittedAgents(DateOnly date, string functionPath);
	}
}