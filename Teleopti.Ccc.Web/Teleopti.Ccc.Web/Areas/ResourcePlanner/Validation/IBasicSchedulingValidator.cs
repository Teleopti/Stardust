namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public interface IBasicSchedulingValidator
	{
		ValidationResult Validate(ValidationParameters parameters);
	}
}