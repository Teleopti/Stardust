using System.Collections.Generic;

namespace StaffHubPoC.Models
{
	public class Group
	{
		public string id { get; set; }
		public string name { get; set; }
		public List<object> memberIds { get; set; }
		public string state { get; set; }
		public string eTag { get; set; }
	}

	public class GroupCollection
	{
		public List<Group> groups { get; set; }
	}
}
