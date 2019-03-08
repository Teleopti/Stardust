namespace Teleopti.Ccc.Domain.Status
{
	public class ExecuteStatusStep
	{
		private readonly AllSteps _allSteps;
		public readonly string NonExistingStepName = "'{0}' is not a known step name";

		public ExecuteStatusStep(AllSteps allSteps)
		{
			_allSteps = allSteps;
		}
		
		public StatusStepResult Execute(string stepName)
		{
			var step = _allSteps.Fetch(stepName);
			return step == null ? 
				new StatusStepResult(false, string.Format(NonExistingStepName, stepName)) : 
				step.Execute();
		}
	}
}