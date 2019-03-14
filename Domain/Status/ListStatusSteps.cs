using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.Status
{
	public class ListStatusSteps
	{
		private readonly AllSteps _allSteps;
		private readonly IFetchCustomStatusSteps _fetchCustomStatusSteps;
		private readonly IConfigReader _configReader;

		public ListStatusSteps(AllSteps allSteps, IFetchCustomStatusSteps fetchCustomStatusSteps, IConfigReader configReader)
		{
			_allSteps = allSteps;
			_fetchCustomStatusSteps = fetchCustomStatusSteps;
			_configReader = configReader;
		}
		
		public IEnumerable<StatusStepInfo> Execute(bool includeFixedSteps)
		{
			var basePath = _configReader.AppConfig("StatusBaseUrl")?.TrimEnd('/');
			var steps = includeFixedSteps ? _allSteps.FetchAll() : _fetchCustomStatusSteps.Execute();
			foreach (var monitorStep in steps)
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