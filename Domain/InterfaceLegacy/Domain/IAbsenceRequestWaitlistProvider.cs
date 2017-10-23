namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceRequestWaitlistProvider
	{
		int GetPositionInWaitlist (IAbsenceRequest absenceRequest);
	}
}