using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AnalysisServices.AdomdClient;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public class CubeRepository
    {
        private string _ASserver;
        private string _ASdatabaseName;
    	private readonly int _defaultTimeZoneId;
    	private string _ASCubeName;

        public CubeRepository(string server, string databaseName, int defaultTimeZoneId)
        {
            _ASserver = server;
            _ASdatabaseName = databaseName;
        	_defaultTimeZoneId = defaultTimeZoneId;
        	_ASCubeName = "Teleopti Analytics";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void Process()
        {

            var processXMLA = string.Format(CultureInfo.InvariantCulture, "<Process xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\"><Object><DatabaseID>{0}</DatabaseID></Object><Type>ProcessFull</Type><WriteBackTableCreation>UseExisting</WriteBackTableCreation></Process>", _ASdatabaseName);
            var setDefaultMembersMDX = string.Format(CultureInfo.InvariantCulture, "ALTER CUBE [{0}] UPDATE DIMENSION [time zone], DEFAULT_MEMBER=[time zone].[{1}]", _ASCubeName, _defaultTimeZoneId);
            var connectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};", _ASserver);
                                  
            using (AdomdConnection connection = new AdomdConnection(connectionString))
            {

				connection.Open();
				connection.ChangeDatabase(_ASdatabaseName);

				using (AdomdCommand cmd = connection.CreateCommand())
				{
					if (connection.Cubes.Count == 0)
					{
						//if never processed, do it once else "setDefaultMembersMDX" will crash
						ExecuteCubeQuery(cmd, processXMLA);
					}

					ExecuteCubeQuery(cmd, setDefaultMembersMDX);
					ExecuteCubeQuery(cmd, processXMLA);
				}
            }
        }

		private void ExecuteCubeQuery(AdomdCommand cmd, string cmdText)
		{
			cmd.CommandText = cmdText;
			cmd.ExecuteNonQuery();
		}
    }
}
