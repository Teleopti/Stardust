using Teleopti.Ccc.Domain.MonitorSystem;

namespace Teleopti.Ccc.DomainTest.MonitorSystemTest
{
	public class FakeMonitorStep : IMonitorStep
	{
		private MonitorStepResult _result;

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