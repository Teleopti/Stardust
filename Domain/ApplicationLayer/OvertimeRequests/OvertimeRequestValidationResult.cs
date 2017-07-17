namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestValidationResult
	{
		public bool IsValid { get; set; }

		public string InvalidReason { get; set; }
	}
}