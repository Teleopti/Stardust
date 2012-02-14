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
        private string[] _argumentCollection;
        private bool _useIntegratedSecurity;
        private bool _forecasterMode;
        private bool _personUpdateMode;

        public CommandLineArgument(string[] argumentCollection)
        {
            _argumentCollection = argumentCollection;
            readArguments();
        }
        
        public bool PersonUpdateMode
        {
            get { return _personUpdateMode; }
        }

        public bool ForecasterMode
        {
            get { return _forecasterMode; }
        }

        public string DestinationServer
        {
            get { return _destinationServer; }
        }

        public string DestinationDatabase
        {
            get { return _destinationDatabase; }
        }

        public string DestinationUserName
        {
            get { return _destinationUserName; }
        }

        public string DestinationPassword
        {
            get { return _destinationPassword; }
        }

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

        public bool UseIntegratedSecurity
        {
            get { return _useIntegratedSecurity; }
        }

        private void readArguments()
        {
            foreach (string s in _argumentCollection)
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
                    case "-FM":
                        _forecasterMode = true;
                        break;
                    case "-PU":
                        _personUpdateMode = true;
                        break;
                }
            }
        }
    }
}