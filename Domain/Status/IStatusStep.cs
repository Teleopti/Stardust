namespace Teleopti.Ccc.Domain.Status
{
	public interface IStatusStep
	{
		StatusStepResult Execute();
		string Name { get; }
		string Description { get; }
	}
}