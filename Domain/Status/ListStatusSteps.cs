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
		
		public IEnumerable<StatusStepInfo> Execute(Uri virtualDirectoryAbsolutePath, string actionString)
		{
			var basePath = virtualDirectoryAbsolutePath.ToString().TrimEnd('/') + "/" + actionString + "/";
			foreach (var monitorStep in _allSteps.FetchAll())
			{
				var stepName = monitorStep.Name;
				yield return new StatusStepInfo(stepName, monitorStep.Description, basePath + stepName);
			}
		}
	}
}