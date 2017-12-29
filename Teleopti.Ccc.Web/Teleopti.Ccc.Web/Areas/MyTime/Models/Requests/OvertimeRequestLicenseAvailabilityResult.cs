namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class OvertimeRequestLicenseAvailabilityResult
	{
		public bool IsLicenseAvailable { get; set; }
		public bool HasPermissionForOvertimeRequests { get; set; }
	}
}