using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public class FetchToggleOverrideFromDb: IFetchToggleOverride
	{
		private readonly IConfigReader _configReader;

		public FetchToggleOverrideFromDb(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public bool? OverridenValue(Toggles toggle)
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
	}
}