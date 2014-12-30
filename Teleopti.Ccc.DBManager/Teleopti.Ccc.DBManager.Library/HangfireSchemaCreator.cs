using System.Data.SqlClient;

namespace Teleopti.Ccc.DBManager.Library
{
	public class HangfireSchemaCreator
	{
		public void ApplyHangfire(SqlConnection sqlConnection)
		{
			var assembly = typeof (Hangfire.SqlServer.SqlServerStorage).Assembly;
			var installerType = assembly.GetType("Hangfire.SqlServer.SqlServerObjectsInstaller");
			var method = installerType.GetMethod("Install");
			method.Invoke(null, new object[] { sqlConnection });
		}
	}
}