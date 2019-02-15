using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.DomainTest.MonitorSystem
{
	public class FakeMonitorStep : IMonitorStep
	{
		private MonitorStepResult _result;

		public FakeMonitorStep()
		{
			_result = new MonitorStepResult(true, string.Empty);
		}
		
		public void SetResult(MonitorStepResult result)
		{
			_result = result;
		}
		
		public MonitorStepResult Execute()
		{
			return _result;
		}

		public string Name { get; set; } = "Fake";
	}
}