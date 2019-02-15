using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class TryExecuteMonitorStep
	{
		private readonly IEnumerable<IMonitorStep> _monitorSteps;
		public readonly string NonExistingStepName = "'{0}' is not a known monitor step";

		public TryExecuteMonitorStep(IEnumerable<IMonitorStep> monitorSteps)
		{
			_monitorSteps = monitorSteps;
		}
		
		public MonitorStepResult TryExecute(string monitorStepName)
		{
			var step = _monitorSteps.SingleOrDefault(x => string.Equals(x.Name, monitorStepName, StringComparison.InvariantCultureIgnoreCase));
			return step == null ? 
				new MonitorStepResult(false, string.Format(NonExistingStepName, monitorStepName)) : 
				step.Execute();
		}
	}
}