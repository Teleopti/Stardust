using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace Teleopti.Ccc.DBManager.Library
{
	public class HangfireSchemaCreator
	{
		// Worth noting:
		//
		// Loading the hangfire assembly dynamically because hangfire is not strong named,
		// and cant be referenced directly from our assemblies because all Teleopti assemblies are strong named.
		//
		// Will try to find the assembly in packages, and copy it to current directory. Used when this is run from test assemblies.
		// If packages cant be found, will assume the hangfire assemblies exists in current directory. Used when running db manager because of pre build event.
		//

		private const string sqlAssemblyName = "Hangfire.SqlServer.dll";
		private const string coreAssemblyName = "Hangfire.Core.dll";

		public void ApplyHangfire(SqlConnection sqlConnection)
		{
			copyHangfireAssembliesUsingBlackMagic();

			var path = Path.Combine(Environment.CurrentDirectory, sqlAssemblyName);
			var assembly = Assembly.LoadFile(path);
			//var assembly = typeof (Hangfire.SqlServer.SqlServerStorage).Assembly;
			var installerType = assembly.GetType("Hangfire.SqlServer.SqlServerObjectsInstaller");
			var method = installerType.GetMethod("Install");
			method.Invoke(null, new object[] { sqlConnection });
		}

		private void copyHangfireAssembliesUsingBlackMagic()
		{
			const string sqlAssemblyPath = @"packages\Hangfire.SqlServer.1.3.0\lib\net45";
			const string coreAssemblyPath = @"packages\Hangfire.Core.1.3.0\lib\net45";

			copyLocalIfFound(sqlAssemblyPath, sqlAssemblyName, @"..\..\..\..\");
			copyLocalIfFound(sqlAssemblyPath, sqlAssemblyName, @"..\..\..\");
			copyLocalIfFound(sqlAssemblyPath, sqlAssemblyName, @"..\..\");

			copyLocalIfFound(coreAssemblyPath, coreAssemblyName, @"..\..\..\..\");
			copyLocalIfFound(coreAssemblyPath, coreAssemblyName, @"..\..\..\");
			copyLocalIfFound(coreAssemblyPath, coreAssemblyName, @"..\..\");
		}

		private void copyLocalIfFound(string assemblyFolder, string assemblyName, string relativePath)
		{
			var path = Path.Combine(relativePath, assemblyFolder, assemblyName);
			if (File.Exists(path))
				File.Copy(path, Path.Combine(Environment.CurrentDirectory, assemblyName), true);
		}


	}
}