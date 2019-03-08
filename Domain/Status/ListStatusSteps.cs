using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Status
{
	public class ListStatusSteps
	{
		private readonly IEnumerable<IStatusStep> _fixedStatusSteps;
		private readonly IFetchCustomStatusSteps _fetchCustomStatusSteps;

		public ListStatusSteps(IEnumerable<IStatusStep> fixedStatusSteps, IFetchCustomStatusSteps fetchCustomStatusSteps)
		{
			_fixedStatusSteps = fixedStatusSteps;
			_fetchCustomStatusSteps = fetchCustomStatusSteps;
		}
		
		public IEnumerable<StatusStepInfo> Execute(Uri virtualDirectoryAbsolutePath, string actionString)
		{
			var steps = _fixedStatusSteps.Union(_fetchCustomStatusSteps.Execute());
			var basePath = virtualDirectoryAbsolutePath.ToString().TrimEnd('/') + "/" + actionString + "/";
			foreach (var monitorStep in steps)
			{
				var stepName = monitorStep.Name;
				yield return new StatusStepInfo(stepName, monitorStep.Description, basePath + stepName);
			}
		}
	}
}