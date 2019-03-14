using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Status
{
	public class ListStatusSteps
	{
		private readonly AllSteps _allSteps;

		public ListStatusSteps(AllSteps allSteps)
		{
			_allSteps = allSteps;
		}
		
		public IEnumerable<StatusStepInfo> Execute(Uri virtualDirectoryAbsolutePath, string statusPath)
		{
			var basePath = virtualDirectoryAbsolutePath.ToString().TrimEnd('/') + "/" + statusPath + "/";
			foreach (var monitorStep in _allSteps.FetchAll())
			{
				var stepName = monitorStep.Name;
				string pingUrl=null;
				var id = 0;
				if (monitorStep is CustomStatusStep customStatusStep)
				{
					pingUrl = basePath + "ping/" + customStatusStep.Name;
					id = customStatusStep.Id;
				}
				yield return new StatusStepInfo(id, stepName, monitorStep.Description, basePath + "check/" + stepName, pingUrl);
			}
		}
	}
}