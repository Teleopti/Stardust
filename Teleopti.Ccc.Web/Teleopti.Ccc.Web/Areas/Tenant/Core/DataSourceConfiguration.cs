﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class DataSourceConfiguration
	{
		public string Tennant { get; set; }
		public string AnalyticsConnectionString { get; set; }
		public IDictionary<string, string> ApplicationNHibernateConfig { get; set; }
	}
}