namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public interface IScheduleValidator
	{
		void FillResult(ValidationResult validationResult, ValidationInput input);
	}
}