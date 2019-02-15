namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public interface IMonitorStep
	{
		MonitorStepResult Execute();
		string Name { get; }
	}
}