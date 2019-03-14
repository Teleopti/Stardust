using System.Collections.Generic;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Domain.Status
{
	public class ListCustomStatusSteps
	{
		private readonly IFetchCustomStatusSteps _fetchCustomStatusSteps;
		private readonly IConfigReader _configReader;

		public ListCustomStatusSteps(IFetchCustomStatusSteps fetchCustomStatusSteps, IConfigReader configReader)
		{
			_fetchCustomStatusSteps = fetchCustomStatusSteps;
			_configReader = configReader;
		}
		
		public IEnumerable<CustomStatusStepInfo> Execute()
		{
			var basePath = _configReader.AppConfig("StatusBaseUrl");
			
			foreach (var statusStep in _fetchCustomStatusSteps.Execute())
			{
				yield return new CustomStatusStepInfo(
					statusStep.Id, 
					statusStep.Name, 
					statusStep.Description, 
					(int) statusStep.Limit.TotalSeconds, 
					basePath + "/ping/" + statusStep.Name
				);
			}
		}
	}
}