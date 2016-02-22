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
						sqlConnection.ConnectionString,
						"Messages", 
						1, 
						new TraceSource("SignalR." + typeof(SqlMessageBus).Name)
					}, null);

			var method = installerType.GetMethod("Install");
			method.Invoke(instantiatedType, new object[] { });
		}
	}
}