using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamViewModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public Guid SiteId { get; set; }
		public int NumberOfAgents { get; set; }
	}
}