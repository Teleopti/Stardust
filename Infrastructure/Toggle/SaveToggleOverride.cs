using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public class SaveToggleOverride
	{
		private readonly IConfigReader _configReader;

		public SaveToggleOverride(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public void Save(Toggles toggle, bool value)
		{
			using (var connection = new SqlConnection(_configReader.ConnectionString("Toggle")))
			{
				connection.Open();
				var sqlCommand = new SqlCommand("insert into Toggle.Override (Enabled, Toggle) values (@value, @name)", connection);
				sqlCommand.Parameters.AddWithValue("@value", value ? 1 : 0);
				sqlCommand.Parameters.AddWithValue("@name", toggle.ToString());
				sqlCommand.ExecuteNonQuery();
			}
		}
	}
}