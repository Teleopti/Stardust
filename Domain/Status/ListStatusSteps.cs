using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Status
{
	public class ListStatusSteps
	{
		private readonly IEnumerable<IStatusStep> _monitorSteps;

		public ListStatusSteps(IEnumerable<IStatusStep> monitorSteps)
		{
			_monitorSteps = monitorSteps;
		}
		
		public IEnumerable<string> Execute()
		{
			return _monitorSteps.Select(x => x.Name);
		}
	}
}