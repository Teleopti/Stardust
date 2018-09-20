using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.ToggleAdmin;

namespace Teleopti.Ccc.Infrastructure.Toggle.Admin
{
	public class FetchAllToggleOverrides : IFetchAllToggleOverrides
	{
		private readonly IConfigReader _configReader;

		public FetchAllToggleOverrides(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public IDictionary<string, bool> OverridenValues()
		{
			using (var connection = new SqlConnection(_configReader.ConnectionString("Toggle")))
			{
				connection.Open();
				using (var sqlCommand = new SqlCommand("select Toggle, Enabled from Toggle.Override", connection))
				{
					var result = new Dictionary<string, bool>();
					using (var reader = sqlCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							var toggleName = reader.GetString(0);
							var toggleValue = reader.GetBoolean(1);
							result.Add(toggleName, toggleValue);
						}
					}
					return result;
				}
			}
		}
	}
}