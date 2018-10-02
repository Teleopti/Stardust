using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle.InApp
{
	public class FetchToggleOverride
	{
		private readonly IConfigReader _configReader;

		public FetchToggleOverride(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public virtual bool? OverridenValue(Toggles toggle)
		{
			var connString = _configReader.ConnectionString("Toggle");
			if (connString == null)
				return null;
			using (var connection = new SqlConnection(connString))
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