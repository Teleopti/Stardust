using System;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class ApplicationUserQuery : IApplicationUserQuery
	{
		//remove "func" when moving out rest of code
		private readonly Func<ITennantDatabaseConnectionFactory> _tennantDatabaseConnectionFactory;

		private const string sql = @"
select auth.Person, auth.Password, ud.LastPasswordChange, ud.InvalidAttemptsSequenceStart, ud.InvalidAttempts
from ApplicationAuthenticationInfo auth
inner join Person p on p.Id=auth.Person
left outer join UserDetail ud on p.Id=ud.Person
where ApplicationLogonName=@userName
and (p.TerminalDate is null or p.TerminalDate>getdate())";

		public ApplicationUserQuery(Func<ITennantDatabaseConnectionFactory> tennantDatabaseConnectionFactory)
		{
			_tennantDatabaseConnectionFactory = tennantDatabaseConnectionFactory;
		}

		public ApplicationUserQueryResult FindUserData(string userName)
		{
			using (var conn = _tennantDatabaseConnectionFactory().CreateConnection())
			{
				using (var cmd = new SqlCommand(sql, conn))
				{
					cmd.Parameters.Add(new SqlParameter("@userName", userName));
					using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SingleResult))
					{
						if (reader.HasRows)
						{
							reader.Read();
							return new ApplicationUserQueryResult
							{
								Success = true,
								PersonId = reader.GetGuid(reader.GetOrdinal("Person")),
								Tennant = "Teleopti WFM", //will be changed and read from db later
								Password = reader.GetString(reader.GetOrdinal("Password")),
								LastPasswordChange = new DateTime(reader.GetDateTime(reader.GetOrdinal("LastPasswordChange")).Ticks, DateTimeKind.Utc),
								InvalidAttemptsSequenceStart = new DateTime(reader.GetDateTime(reader.GetOrdinal("InvalidAttemptsSequenceStart")).Ticks, DateTimeKind.Utc),
								InvalidAttempts = reader.GetInt32(reader.GetOrdinal("InvalidAttempts"))
							};
						}
						return new ApplicationUserQueryResult { Success = false };
					}
				}
			}
		}
	}
}