namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestAvailability
	{
		bool IsEnabled();
		bool IsLicenseEnabled();
	}
}