using System.IO;
using System.Text;
using log4net;
using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.AnalysisServices;

namespace AnalysisServicesManager
{
    public class Repository
    {
	    private readonly CubeSourceFormat _cubeSourceFormat;
	    private readonly ServerConnectionInfo _analysisConnectionInfo;
		public const string CubeName = "Teleopti Analytics";

		private static readonly ILog Logger = LogManager.GetLogger(typeof(Repository));

        public Repository(CubeSourceFormat cubeSourceFormat, ServerConnectionInfo analysisConnectionInfo)
        {
	        _cubeSourceFormat = cubeSourceFormat;
	        _analysisConnectionInfo = analysisConnectionInfo;
        }

	    public void ExecuteAnyXmla(string filePath)
	    {
		    string preScript = File.ReadAllText(filePath, Encoding.Unicode);
			string postScript = _cubeSourceFormat.FindAndReplace(preScript);

		    using (var connection = new AdomdConnection(_analysisConnectionInfo.ConnectionString))
		    {
			    connection.Open();
			    using (var cmd = connection.CreateCommand())
			    {
				    cmd.CommandText = postScript;
				    cmd.ExecuteNonQuery();
			    }
		    }
	    }

	    public void ProcessCube()
		{
			using (Server server = new Server())
			{
				server.Connect(_analysisConnectionInfo.ConnectionString);
				Database database = server.Databases.FindByName(_analysisConnectionInfo.DatabaseName);
				Cube cube = database.Cubes.FindByName(CubeName);
				cube.Process(ProcessType.ProcessFull);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Logger.Info(System.String)")]
		public void DropDatabase()
		{
			using (Server server = new Server())
			{
				server.Connect(_analysisConnectionInfo.ConnectionString);

				Database database = server.Databases.FindByName(_analysisConnectionInfo.DatabaseName);

				if (database != null)
				{
					database.Drop();
				}
				else
				{
					Logger.Info("   Can't drop database, database does not yet exist.");
				}
			}
		}
    }
}
