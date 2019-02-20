using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Status
{
	public class ListStatusSteps
	{
		private readonly IEnumerable<IStatusStep> _monitorSteps;

		public ListStatusSteps(IEnumerable<IStatusStep> monitorSteps)
		{
			_monitorSteps = monitorSteps;
		}
		
		public IEnumerable<StatusStepInfo> Execute(Uri virtualDirectoryAbsolutePath, string actionString)
		{
			var basePath = virtualDirectoryAbsolutePath.ToString().TrimEnd('/') + "/" + actionString + "/";
			foreach (var monitorStep in _monitorSteps)
			{
				var stepName = monitorStep.Name;
				yield return new StatusStepInfo(stepName, monitorStep.Description, basePath + stepName);
			}
		}
	}
}