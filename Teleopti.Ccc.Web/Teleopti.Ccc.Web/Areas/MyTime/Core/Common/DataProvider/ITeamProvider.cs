using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ITeamProvider
	{
		IEnumerable<ITeam> GetPermittedTeams(DateOnly date, string functionPath);
		IEnumerable<ITeam> GetPermittedNotEmptyTeams(DateOnly date, string functionPath);
	}
}