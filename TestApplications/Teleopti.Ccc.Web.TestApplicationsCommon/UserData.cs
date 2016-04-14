using System.Collections.Generic;

namespace Teleopti.Ccc.Web.TestApplicationsCommon
{
	public class UserData
	{
		public string Username { get; set; }
		public string Password { get; set; } 

		public static List<UserData> TestUsers = new List<UserData>
		{
			new UserData {Username = "demo", Password = "demo"},
			new UserData {Username = "pa", Password = "pa"},
			new UserData {Username = "aa", Password = "aa"}
		};
	}
}