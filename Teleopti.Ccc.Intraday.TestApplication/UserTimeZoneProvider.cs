using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Intraday.TestApplication.Infrastructure;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	public class UserTimeZoneProvider
	{
		private readonly string _connectionString;

		public UserTimeZoneProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		public UserTimeZoneInfo GetTimeZoneForCurrentUser()
		{
			var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			var dbCommand = new DatabaseCommand(CommandType.Text,
				$"select * from Person p inner join Tenant.PersonInfo pi on pi.Id = p.Id where pi.[Identity] = '{currentUser}'", _connectionString);
			var result = dbCommand.ExecuteDataSet(new SqlParameter[0]);
			if (result.Tables[0].Rows.Count == 0)
			{
				return null;
			}
			var username = result.Tables[0].Rows[0]["ApplicationLogonName"].ToString();
			var timezone = result.Tables[0].Rows[0]["DefaultTimeZone"].ToString();
			return new UserTimeZoneInfo(username, timezone);

		}

		public UserTimeZoneInfo GetTimeZoneForUser(string username)
		{
			var dbCommand = new DatabaseCommand(CommandType.Text,
				$"select DefaultTimeZone from Person p inner join Tenant.PersonInfo pi on pi.Id = p.Id where pi.[ApplicationLogonName] = '{username}'", _connectionString);
			var result = dbCommand.ExecuteDataSet(new SqlParameter[0]);
			if (result.Tables[0].Rows.Count == 0)
			{
				return null;
			}
			var timezone = result.Tables[0].Rows[0]["DefaultTimeZone"].ToString();
			return new UserTimeZoneInfo(username, timezone);
		}
	}


	public class UserTimeZoneInfo
	{
		public UserTimeZoneInfo(string username, string timeZoneId)
		{
			Username = username;
			TimeZoneId = timeZoneId;
		}

		public string Username { get; set; }
		public string TimeZoneId { get; set; }
	}
}