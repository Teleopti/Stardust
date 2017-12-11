namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestValidator
	{
		OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context);
	}
}