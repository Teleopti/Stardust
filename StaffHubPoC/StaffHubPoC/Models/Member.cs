using System.Collections.Generic;

namespace StaffHubPoC.Models
{
	public class Member
	{
		public string id { get; set; }
		public string userId { get; set; }
		public string displayName { get; set; }
		public string firstName { get; set; }
		public string lastName { get; set; }
		public string email { get; set; }
		public object phoneNumber { get; set; }
		public bool isManager { get; set; }
		public List<string> groupIds { get; set; }
		public string state { get; set; }
		public string eTag { get; set; }
	}

	public class MemberCollection
	{
		public List<Member> members { get; set; }
	}
}
