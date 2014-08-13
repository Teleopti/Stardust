using System;
using System.Globalization;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices;

namespace AnalysisServicesManager
{
    public class Repository
    {
		private string _connectionString;
		private string _databaseName;
		private const string CubeName = "Teleopti Analytics";

        public Repository(CommandLineArgument connectionString)
        {
            _connectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};", connectionString.AnalysisServer);
			_databaseName = connectionString.AnalysisDatabase;
			
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xmla"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void ExecuteAnyXmla(CommandLineArgument argument)
        {
			if (argument != null)
			{
				string preScript = "";
				if (argument.CustomFilePath != null)
				{
					preScript = new FileHandler(argument.CustomFilePath).FileAsString;
				}
				else
				{
					preScript = new FileHandler(argument.FilePath).FileAsString;
				}

				string postScript = new CubeSourceFormat(preScript).FindAndReplace(argument);

				using (AdomdConnection connection = new AdomdConnection(_connectionString))
				{
					connection.Open();
					using (AdomdCommand cmd = connection.CreateCommand())
					{
						cmd.CommandText = postScript;
						cmd.ExecuteNonQuery();
					}
				}
			}
        }

		public void ProcessCube()
		{
			using (Server server = new Server())
			{
				server.Connect(_connectionString);
				Database database = server.Databases.FindByName(_databaseName);
				Cube cube = database.Cubes.FindByName(CubeName);
				cube.Process(ProcessType.ProcessFull);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
		public void DropDatabase()
		{
			using (Server server = new Server())
			{
				server.Connect(_connectionString);

				Database database = server.Databases.FindByName(_databaseName);

				if (database != null)
				{
					database.Drop();
				}
				else
				{
					Console.WriteLine("   Can't drop database, database does not yet exist.");
				}
			}
		}
    }
}
