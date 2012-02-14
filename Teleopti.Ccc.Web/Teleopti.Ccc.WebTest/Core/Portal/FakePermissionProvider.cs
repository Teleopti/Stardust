using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	public class FakePermissionProvider : IPermissionProvider
	{
		public bool HasApplicationFunctionPermission(string applicationFunctionPath) { return true; }
		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person) { return true; }
		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team) { return true; }
	}
}