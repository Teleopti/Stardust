using System.Collections.Generic;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.Status
{
	public class ListStatusSteps
	{
		private readonly AllSteps _allSteps;
		private readonly IConfigReader _configReader;

		public ListStatusSteps(AllSteps allSteps, IConfigReader configReader)
		{
			_allSteps = allSteps;
			_configReader = configReader;
		}
		
		public IEnumerable<StatusStepInfo> Execute()
		{
			var basePath = _configReader.AppConfig("StatusBaseUrl");
			foreach (var monitorStep in _allSteps.FetchAll())
			{
				var stepName = monitorStep.Name;
				yield return new StatusStepInfo(stepName, monitorStep.Description, basePath + "/check/" + stepName);
			}
		}
	}
}