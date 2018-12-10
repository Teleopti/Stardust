using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;


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
			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence, date, person))
				return string.Format(Resources.NoPermissionAddFullDayAbsenceForAgent, person.Name);

			if (!_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths
					.ModifyWriteProtectedSchedule) && person.PersonWriteProtection.IsWriteProtected(date))
				return Resources.WriteProtectSchedule;

			if (!_permissionProvider.IsPersonSchedulePublished(date, person) && !_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, date, person))
				return string.Format(Resources.NoPermissionToEditUnpublishedSchedule, person.Name);
			return null;
		}

		public string CheckAddIntradayAbsenceForPerson(IPerson person, DateOnly date)
		{
			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence, date, person))
				return string.Format(Resources.NoPermisionAddIntradayAbsenceForAgent, person.Name);
			if (!_permissionProvider.IsPersonSchedulePublished(date, person) && !_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, date, person))
				return string.Format(Resources.NoPermissionToEditUnpublishedSchedule, person.Name);
			return null;
		}
	}
}