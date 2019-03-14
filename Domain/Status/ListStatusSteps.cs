using System;
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
			var basePath = _configReader.AppConfig("StatusBaseUrl")?.TrimEnd('/');
			foreach (var monitorStep in _allSteps.FetchAll())
			{
				var stepName = monitorStep.Name;
				string pingUrl=null;
				var id = 0;
				if (monitorStep is CustomStatusStep customStatusStep)
				{
					pingUrl = basePath + "/ping/" + customStatusStep.Name;
					id = customStatusStep.Id;
				}
				yield return new StatusStepInfo(id, stepName, monitorStep.Description, basePath + "/check/" + stepName, pingUrl);
			}
		}
	}
}