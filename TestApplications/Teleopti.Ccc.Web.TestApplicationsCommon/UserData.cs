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
			new UserData {Username = "aa", Password = "aa"},
			new UserData {Username = "pb", Password = "pb"},
			new UserData {Username = "jb", Password = "jb"},
			new UserData {Username = "nb", Password = "nb"},
			new UserData {Username = "ac", Password = "ac"},
			new UserData {Username = "sc", Password = "sc"},
			new UserData {Username = "sm", Password = "sm"},
			new UserData {Username = "cj", Password = "cj"},
			new UserData {Username = "jl", Password = "jl"},
			new UserData {Username = "kg", Password = "kg"},
			new UserData {Username = "jq", Password = "jq"},
			new UserData {Username = "jf", Password = "jf"},
			new UserData {Username = "bg", Password = "bg"},
			new UserData {Username = "jk2", Password = "jk2"},
			new UserData {Username = "jk", Password = "jk"},
			new UserData {Username = "rk", Password = "rk"},
			new UserData {Username = "dp", Password = "dp"},
			new UserData {Username = "co", Password = "co"},
			new UserData {Username = "lk", Password = "lk"}
		};
	}
}