using System.Collections.Generic;

namespace Teleopti.Wfm.Administration.Models
{
	public struct JobStepModel
	{
		public string Name;
		public bool DependsOnTenant;
	}

	public class JobCollectionModel
	{
		public string JobName { get; set; }
		public IList<JobStepModel> JobSteps { get; set; }
		public bool NeedsParameterDataSource { get; set; }
		public IList<string> NeededDatePeriod { get; set; }
	}
}