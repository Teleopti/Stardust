using System;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class IdentityUserQuery : IIdentityUserQuery
	{
		//remove "func" when moving out rest of code
		private readonly Func<ITennantDatabaseConnectionFactory> _tennantDatabaseConnectionFactory;

		private const string sql = @"
select auth.Person from AuthenticationInfo auth
inner join Person p on p.Id=auth.Person
WHERE auth.[Identity] = @identity
and (p.TerminalDate is null or p.TerminalDate>getdate())";

		public IdentityUserQuery(Func<ITennantDatabaseConnectionFactory> tennantDatabaseConnectionFactory)
		{
			_tennantDatabaseConnectionFactory = tennantDatabaseConnectionFactory;
		}

		public ApplicationUserQueryResult FindUserData(string identity)
		{
			using (var conn = _tennantDatabaseConnectionFactory().CreateConnection())
			{
				using (var cmd = new SqlCommand(sql, conn))
				{
					cmd.Parameters.Add(new SqlParameter("@identity", identity));
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
							};
						}
						return new ApplicationUserQueryResult { Success = false };
					}
				}
			}
		}
	}
}