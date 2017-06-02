namespace Teleopti.Ccc.Domain.ResourceCalculation.Validation
{
	public interface IBasicSchedulingValidator
	{
		ValidationResult Validate(ValidationParameters parameters);
	}
}