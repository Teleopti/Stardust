using System.Globalization;
using Microsoft.AnalysisServices.AdomdClient;


namespace AnalysisServicesManager
{
    public class Repository
    {
        public Repository()
        {
            
        }

        public Repository(CommandLineArgument connectionString)
        {
            _connectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};", connectionString.AnalysisServer);
        }

        private string _connectionString;

        public void Execute(string cubeSource)
        {
            using (AdomdConnection connection = new AdomdConnection(_connectionString))
            {
                connection.Open();
                using (AdomdCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = cubeSource;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
