using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class PersonOrganizationReader : IPersonOrganizationReader
	{
		private readonly INow _now;
		private readonly string _applicationConnectionString;
		private const string sqlQuery = "exec [dbo].[LoadAllPersonsCurrentBuSiteTeam] @now";

		public PersonOrganizationReader(INow now, string applicationConnectionString)
		{
			_now = now;
			_applicationConnectionString = applicationConnectionString;
		}

		public IEnumerable<PersonOrganizationData> LoadAll()
		{
			var ret = new List<PersonOrganizationData>();
			//inject ICurrentUnitOfWork and handle transaction from outside later
			//rta client needs to be aware of IUnitOfWork first!
			using(var conn = new SqlConnection(_applicationConnectionString))
			{
				conn.Open();
				using (var cmd = new SqlCommand(sqlQuery, conn))
				{
					cmd.Parameters.AddWithValue("@now", _now.UtcDateTime());
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							ret.Add(new PersonOrganizationData
								{
									PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
									TeamId = reader.GetGuid(reader.GetOrdinal("TeamId")),
									SiteId = reader.GetGuid(reader.GetOrdinal("SiteId"))
								});
						}
					}
				}
			}
			return ret;
		}
	}
}