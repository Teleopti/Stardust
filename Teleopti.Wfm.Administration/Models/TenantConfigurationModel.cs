using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Wfm.Administration.Models
{
	public class TenantConfigurationModel		
	{
		public string ConnectionString { get; set; }

		public IBaseConfiguration BaseConfig { get; set; }

		public string TenantName { get; set; }

		public bool IsBaseConfigured { get; set; }
	}
}