using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsGroupPage
	{
		public int GroupPageId { get; set; }
		public Guid GroupPageCode { get; set; }
		public string GroupPageName { get; set; }
		public string GroupPageNameResourceKey { get; set; }
	}
}