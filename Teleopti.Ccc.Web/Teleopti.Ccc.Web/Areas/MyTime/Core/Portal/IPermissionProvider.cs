using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public interface IPermissionProvider
	{
		bool HasApplicationFunctionPermission(string applicationFunctionPath);
		bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person);
		bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team);
	}
}