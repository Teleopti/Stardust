using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsSite
	{
		public Guid SiteCode { get; set; }
		public string Name { get; set; }
		public DateTime? DataSourceUpdateDate { get; set; }
	}
}