using Teleopti.Ccc.Domain.Status;

namespace Teleopti.Ccc.DomainTest.Status
{
	public class FakeStatusStep : IStatusStep
	{
		private StatusStepResult _result;

		public FakeStatusStep()
		{
			_result = new StatusStepResult(true, string.Empty);
		}
		
		public void SetResult(StatusStepResult result)
		{
			_result = result;
		}
		
		public StatusStepResult Execute()
		{
			return _result;
		}

		public string Name { get; set; } = "FakeName";
		public string Description { get; set; } = "FakeDesc";
	}
}