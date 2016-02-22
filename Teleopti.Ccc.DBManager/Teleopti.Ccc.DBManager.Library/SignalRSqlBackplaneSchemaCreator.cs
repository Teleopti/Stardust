using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.AspNet.SignalR.SqlServer;

namespace Teleopti.Ccc.DBManager.Library
{
	public class SignalRSqlBackplaneSchemaCreator
	{
		public void ApplySignalRSqlBackplane(SqlConnection sqlConnection)
		{
			var assembly = typeof(SqlMessageBus).Assembly;
			var installerType = assembly.GetType("Microsoft.AspNet.SignalR.SqlServer.SqlInstaller");

			var instantiatedType =
				Activator.CreateInstance(installerType,
					System.Reflection.BindingFlags.Public |
					System.Reflection.BindingFlags.Instance,
					null, new object[]
					{
						sqlConnectionToConnectionString(sqlConnection),
						"Messages", 
						1, 
						new TraceSource("SignalR." + typeof(SqlMessageBus).Name)
					}, null);

			var method = installerType.GetMethod("Install");
			method.Invoke(instantiatedType, new object[] { });
		}

		private static string sqlConnectionToConnectionString(SqlConnection conn)
		{
			var property = conn.GetType().GetProperty("ConnectionOptions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			var optionsObject = property.GetValue(conn, null);
			var method = optionsObject.GetType().GetMethod("UsersConnectionString");
			var connStr = method.Invoke(optionsObject, new object[] { false }) as string; // argument is "hidePassword" so we set it to false
			return connStr;
		}
	}
}