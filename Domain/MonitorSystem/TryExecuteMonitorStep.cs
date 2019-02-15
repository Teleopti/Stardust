using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class TryExecuteMonitorStep
	{
		private readonly IEnumerable<IMonitorStep> _monitorSteps;

		public TryExecuteMonitorStep(IEnumerable<IMonitorStep> monitorSteps)
		{
			_monitorSteps = monitorSteps;
		}
		
		public bool TryExecute(string monitorStepName, out MonitorStepResult result)
		{
			var step = _monitorSteps.SingleOrDefault(x => string.Equals(x.Name, monitorStepName, StringComparison.InvariantCultureIgnoreCase));
			if (step == null)
			{
				result = null;
				return false;
			}
			result = step.Execute();
			return true;
		}
	}
}