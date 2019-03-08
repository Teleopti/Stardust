using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Status
{
	public class ExecuteStatusStep
	{
		private readonly IEnumerable<IStatusStep> _statusSteps;
		private readonly IFetchCustomStatusSteps _fetchCustomStatusSteps;
		public readonly string NonExistingStepName = "'{0}' is not a known step name";

		public ExecuteStatusStep(IEnumerable<IStatusStep> statusSteps, IFetchCustomStatusSteps fetchCustomStatusSteps)
		{
			_statusSteps = statusSteps;
			_fetchCustomStatusSteps = fetchCustomStatusSteps;
		}
		
		public StatusStepResult Execute(string monitorStepName)
		{
			var steps = _statusSteps.Union(_fetchCustomStatusSteps.Execute());
			var step = steps.SingleOrDefault(x => string.Equals(x.Name, monitorStepName, StringComparison.InvariantCultureIgnoreCase));
			return step == null ? 
				new StatusStepResult(false, string.Format(NonExistingStepName, monitorStepName)) : 
				step.Execute();
		}
	}
}