using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Configuration;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Wfm.Administration.Models
{
	public class TenantConfigurationModel		
	{
		public BaseConfiguration BaseConfig { get; set; }

		public string TenantName { get; set; }

		public bool IsBaseConfigured { get; set; }
	}

	public class TenantConfigurationOption
	{
		public List<LookupIntegerItem> CultureList { get; set; }
		public List<LookupIntegerItem> IntervalLengthList { get; set; }
		public List<LookupStringItem> TimeZoneList { get; set; }
	}
}