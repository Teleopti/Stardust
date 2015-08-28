using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class PersonOrganizationReader : IPersonOrganizationReader
	{
		private readonly INow _now;
		private readonly IConnectionStrings _connectionStrings;
		private const string sqlQuery = "exec [dbo].[LoadAllPersonsCurrentBuSiteTeam] @now";

		public PersonOrganizationReader(INow now, IConnectionStrings connectionStrings)
		{
			_now = now;
			_connectionStrings = connectionStrings;
		}

		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			var ret = new List<PersonOrganizationData>();
			using(var conn = new SqlConnection(_connectionStrings.Application()))
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