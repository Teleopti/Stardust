using System.Collections.Generic;

namespace Teleopti.Wfm.Administration.Models
{
	public class JobCollectionModel
	{
		public string JobName { get; set; }
		public IList<string> JobStepNames { get; set; }
		public bool NeedsParameterDataSource { get; set; }
		public IList<string> NeededDatePeriods { get; set; }
	}
}