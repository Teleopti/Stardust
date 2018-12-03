using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule
{
	public interface IPermissionChecker
	{
		string CheckAddFullDayAbsenceForPerson(IPerson person, DateOnly date);
		string CheckAddIntradayAbsenceForPerson(IPerson person, DateOnly date);
	}
}