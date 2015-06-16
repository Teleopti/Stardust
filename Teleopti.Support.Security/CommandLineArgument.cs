using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Support.Security
{
	public class CommandLineArgument
	{
        private string _destinationServer;
        private string _destinationDatabase;
        private string _destinationUserName;
        private string _destinationPassword;
        private bool _useIntegratedSecurity;

		public CommandLineArgument(string[] argumentCollection)
        {
						readArguments(argumentCollection);
        }

		public string AggDatabase { get; private set; }

		public string DestinationConnectionString
        {
            get
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
                sqlConnectionStringBuilder.DataSource = _destinationServer;
                sqlConnectionStringBuilder.InitialCatalog = _destinationDatabase;

                if (_useIntegratedSecurity)
                {
                    sqlConnectionStringBuilder.IntegratedSecurity = _useIntegratedSecurity;
                }
                else
                {
                    sqlConnectionStringBuilder.UserID = _destinationUserName;
                    sqlConnectionStringBuilder.Password = _destinationPassword;
                }
                return sqlConnectionStringBuilder.ConnectionString;
            }
        }

				private void readArguments(string[] argumentCollection)
        {
					foreach (string s in argumentCollection)
            {
                string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
                string switchValue = s.Remove(0, 3);

                switch (switchType)
                {
                    case "-DS":   // Destination Server Name.
                        _destinationServer = switchValue;
                        break;
                    case "-DD":   // Destination Database Name.
                        _destinationDatabase = switchValue;
                        break;
                    case "-DU":   // Destination User Name.
                        _destinationUserName = switchValue;
                        break;
                    case "-DP":   // Destination Password.
                        _destinationPassword = switchValue;
                        break;
                    case "-EE":
                        _useIntegratedSecurity = true;
                        break;
					case "-CD":   // Cross Db Name
						AggDatabase = switchValue;
						break;
                }
            }
        }
    }
}