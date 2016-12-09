﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Loadtest
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
			new UserData {Username = "jk", Password = "jk"},
			new UserData {Username = "jk2", Password = "jk2"},
			//new UserData {Username = "jb", Password = "jb"},
			//new UserData {Username = "nb", Password = "nb"},
			//new UserData {Username = "ac", Password = "ac"},
			//new UserData {Username = "sc", Password = "sc"},
			//new UserData {Username = "sm", Password = "sm"},
			//new UserData {Username = "cj", Password = "cj"},
			//new UserData {Username = "jl", Password = "jl"},
			//new UserData {Username = "kg", Password = "kg"},
			//new UserData {Username = "jq", Password = "jq"},
			//new UserData {Username = "jf", Password = "jf"},
			//new UserData {Username = "bg", Password = "bg"},
			//new UserData {Username = "rk", Password = "rk"},
			//new UserData {Username = "dp", Password = "dp"},
			//new UserData {Username = "co", Password = "co"},
			//new UserData {Username = "lk", Password = "lk"}
		};


		public static List<UserData> GenerateTestUsers(int number)
		{
			var testUsers = new List<UserData>();
			for (var i = 1; i <= number; i++)
			{
				testUsers.Add(new UserData {Username = i.ToString(), Password = i.ToString()});
			}
			return testUsers;
		}
	}
}