using System;
using System.Configuration;

namespace Teleopti.Analytics.Portal.AnalyzerProxy
{
    public class OlapInformation
    {
        private string _olapServer;
        private string _olapDatabase;

        public OlapInformation()
        {
            SetOlapInformation();
        }

        public string OlapServer
        {
            get { return _olapServer; }
        }

        public string OlapDatabase
        {
            get { return _olapDatabase; }
        }

        private void SetOlapInformation()
        {
            ConnectionStringSettings cubeConnectionString = ConfigurationManager.ConnectionStrings["Cube"];

            if (cubeConnectionString != null && !string.IsNullOrEmpty(cubeConnectionString.ConnectionString))
            {
                string[] splittedString1 = cubeConnectionString.ConnectionString.Split(";".ToCharArray());
                foreach (string stringPart in splittedString1)
                {
                    string[] splittedString2 = stringPart.Split("=".ToCharArray());
                    if (splittedString2[0].Equals("DATA SOURCE",StringComparison.InvariantCultureIgnoreCase))
                    {
                        _olapServer = splittedString2[1];
                    }
                    if (splittedString2[0].Equals("INITIAL CATALOG",StringComparison.InvariantCultureIgnoreCase))
                    {
                        _olapDatabase = splittedString2[1];
                    }
                }
            }
        }
    }
}
