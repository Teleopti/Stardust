namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public interface IBasicSchedulingValidator
	{
		ValidationResult Validate(ValidationParameters parameters);
	}
}