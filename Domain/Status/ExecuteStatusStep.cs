using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Status
{
	public class ExecuteStatusStep
	{
		private readonly IEnumerable<IStatusStep> _statusSteps;
		public readonly string NonExistingStepName = "'{0}' is not a known step name";

		public ExecuteStatusStep(IEnumerable<IStatusStep> statusSteps)
		{
			_statusSteps = statusSteps;
		}
		
		public StatusStepResult Execute(string monitorStepName)
		{
			var step = _statusSteps.SingleOrDefault(x => string.Equals(x.Name, monitorStepName, StringComparison.InvariantCultureIgnoreCase));
			return step == null ? 
				new StatusStepResult(false, string.Format(NonExistingStepName, monitorStepName)) : 
				step.Execute();
		}
	}
}