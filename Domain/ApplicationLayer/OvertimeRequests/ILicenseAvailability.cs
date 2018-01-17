namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface ILicenseAvailability
	{
		bool IsLicenseEnabled(string licensePath);
	}
}