using System;
using System.Data.SqlClient;
using System.Reflection;

namespace Teleopti.Ccc.DBManager.Library
{
	public class HangfireSchemaCreator
	{
		public void ApplyHangfire(SqlConnection sqlConnection)
		{
			var assembly = typeof (Hangfire.SqlServer.SqlServerStorage).Assembly;
			var installerType = assembly.GetType("Hangfire.SqlServer.SqlServerObjectsInstaller");
			var method = installerType.GetMethod("Install", BindingFlags.Public | BindingFlags.Static, null, new Type[] {typeof (SqlConnection)}, null);
			method.Invoke(null, new object[] { sqlConnection });
		}
	}
}