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
		
		public IEnumerable<StatusStepInfo> Execute(string baseUrl)
		{
			var startUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
			foreach (var monitorStep in _monitorSteps)
			{
				var stepName = monitorStep.Name;
				yield return new StatusStepInfo(stepName, startUrl + stepName);
			}
		}
	}
}