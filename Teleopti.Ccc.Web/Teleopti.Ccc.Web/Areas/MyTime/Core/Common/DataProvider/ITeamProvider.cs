using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ITeamProvider
	{
		IEnumerable<ITeam> GetPermittedTeams(DateOnly date, string functionPath);
	}
}