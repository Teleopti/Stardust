using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule
{
	public class PermissionChecker : IPermissionChecker
	{
		private readonly IPermissionProvider _permissionProvider;

		public PermissionChecker(IPermissionProvider permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		public string CheckAddFullDayAbsenceForPerson(IPerson person, DateOnly date)
		{
			if (_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, date, person)) return null;
			var ret = string.Format(Resources.NoPermissionAddFullDayAbsenceForAgent, person.Name);
			return ret;
		}

		public string CheckAddIntradayAbsenceForPerson(IPerson person, DateOnly date)
		{
			if (_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, date, person)) return null;
			var ret = string.Format(Resources.NoPermisionAddIntradayAbsenceForAgent, person.Name);
			return ret;
		}
	}
}