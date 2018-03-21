using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Wfm.Administration.Models
{
	public class TenantConfiguration		
	{
		public string TenantName { get; set; }

		public BaseConfiguration BaseConfig { get; set; }
	}
}