using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter
{
	public class FetchPersonIdAndTimeZone
	{
		private readonly string _connectionString;

		public FetchPersonIdAndTimeZone(string connectionString)
		{
			_connectionString = connectionString;
		}

		public IEnumerable<Tuple<Guid, TimeZoneInfo>> ForAllPersons()
		{
			var ret = new List<Tuple<Guid, TimeZoneInfo>>();
			using (var conn = new SqlConnection(_connectionString))
			{
				conn.Open();
				using (var cmd = new SqlCommand("select id, defaulttimezone from person", conn))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							ret.Add(new Tuple<Guid, TimeZoneInfo>(reader.GetGuid(reader.GetOrdinal("id")),
																										TimeZoneInfo.FindSystemTimeZoneById(reader.GetString(reader.GetOrdinal("defaulttimezone")))));
						}
					}
				}
			}
			return ret;
		}
	}
}