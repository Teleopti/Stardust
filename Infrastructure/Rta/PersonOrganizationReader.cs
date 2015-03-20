using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class PersonOrganizationReader : IPersonOrganizationReader
	{
		private readonly INow _now;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
		private const string sqlQuery = "exec [dbo].[LoadAllPersonsCurrentBuSiteTeam] @now";

		public PersonOrganizationReader(INow now, IDatabaseConnectionStringHandler databaseConnectionStringHandler)
		{
			_now = now;
			_databaseConnectionStringHandler = databaseConnectionStringHandler;
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData(string tenant)
		{
			var ret = new List<PersonOrganizationData>();
			//inject ICurrentUnitOfWork and handle transaction from outside later
			//rta client needs to be aware of IUnitOfWork first!
			using (var conn = new SqlConnection(_databaseConnectionStringHandler.AppConnectionString(tenant)))
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