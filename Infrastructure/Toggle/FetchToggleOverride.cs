using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public class FetchToggleOverride: IFetchToggleOverride
	{
		private readonly IConfigReader _configReader;

		public FetchToggleOverride(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public virtual bool? OverridenValue(Toggles toggle)
		{
			using (var connection = new SqlConnection(_configReader.ConnectionString("Toggle")))
			{
				connection.Open();
				var sqlCommand = new SqlCommand("select enabled from Toggle.Override where Toggle = @name", connection);
				sqlCommand.Parameters.AddWithValue("@name", toggle.ToString());
				var result = sqlCommand.ExecuteScalar();
				return (bool?) result;
			}
		}
		
		public Dictionary<string, bool> OverridenValues()
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
	
	//REMOVE THESE WHEN FEATURE IS DONE 
	public interface IFetchToggleOverride
	{
		bool? OverridenValue(Toggles toggle);
		Dictionary<string, bool> OverridenValues();
	}

	public class NoFetchingOfOverridenToggles : IFetchToggleOverride
	{
		public bool? OverridenValue(Toggles toggle)
		{
			return null;
		}

		public Dictionary<string, bool> OverridenValues()
		{
			return new Dictionary<string, bool>();
		}
	}
	//
}