using System.Collections.Generic;

namespace StaffHubPoC.Models
{
	public class Team
	{
		public string id { get; set; }
		public string name { get; set; }
		public string eTag { get; set; }
		public object provisioningDomain { get; set; }
	}

	public class TeamCollection
	{
		public List<Team> teams { get; set; }
	}
}
