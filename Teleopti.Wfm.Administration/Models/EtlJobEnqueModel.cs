using System.Collections.Generic;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.Models
{
	public class EtlJobEnqueModel
	{
		public string JobName { get; set; }
		public IList<JobPeriodDate> JobPeriods{ get; set; }
		public int LogDataSourceId { get; set; }
		public string TenantName { get; set; }
	}
}