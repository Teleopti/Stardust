namespace Teleopti.Ccc.WebTest.Areas.Start
{
	using System.Collections.Generic;

	using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
	using Teleopti.Interfaces.Domain;

	public class FakePermissionProvider : IPermissionProvider
	{
		private readonly IList<string> _grantedFunctions;

		public FakePermissionProvider(IList<string> grantedFunctions)
		{
			this._grantedFunctions = grantedFunctions;
		}

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			return this._grantedFunctions.Contains(applicationFunctionPath);
		}
		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person) { return false; }
		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team) { return false; }
	}
}