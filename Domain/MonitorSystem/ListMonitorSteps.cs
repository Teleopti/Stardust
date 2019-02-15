using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class ListMonitorSteps
	{
		private readonly IEnumerable<IMonitorStep> _monitorSteps;

		public ListMonitorSteps(IEnumerable<IMonitorStep> monitorSteps)
		{
			_monitorSteps = monitorSteps;
		}
		
		public IEnumerable<string> Execute()
		{
			return _monitorSteps.Select(x => x.Name);
		}
	}
}