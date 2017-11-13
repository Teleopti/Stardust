namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestValidationResult
	{
		public bool IsValid { get; set; }

		public bool ShouldDenyIfInValid { get; set; } = true;

		public string InvalidReason { get; set; }
	}
}