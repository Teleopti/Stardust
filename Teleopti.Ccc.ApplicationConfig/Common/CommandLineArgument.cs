using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Common
{
    public class CommandLineArgument : ICommandLineArgument
    {
        private string _sourceServer;
        private string _sourceDatabase;
        private string _sourceUserName;
        private string _sourcePassword;
        private string _destinationServer;
        private string _destinationDatabase;
        private string _destinationUserName;
        private string _destinationPassword;
        private TimeZoneInfo _timeZone = (TimeZoneInfo.Local);
        private DateTime _fromDate;
        private DateTime _toDate;
        private string _businessUnit;
        private string[] _argumentCollection;
        private CultureInfo _cultureInfo = CultureInfo.GetCultureInfo("en-US"); //en-US = standard
        private int _defaultResolution;
        private bool _useIntegratedSecurity;
        private bool _onlyRunMergeDefaultResolution;
    	private string _newUserName;
    	private string _newUserPassword;

    	public CommandLineArgument(string[] argumentCollection)
        {
            _argumentCollection = argumentCollection;
            readArguments();
        }

        #region properties

        public string SourceServer
        {
            get { return _sourceServer; }
        }

        public string SourceDatabase
        {
            get { return _sourceDatabase; }
        }

        public string SourceUserName
        {
            get { return _sourceUserName; }
        }

        public string SourcePassword
        {
            get { return _sourcePassword; }
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

        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
        }

        public DateTime FromDate
        {
            get { return _fromDate; }
        }

        public DateTime ToDate
        {
            get { return _toDate; }
        }

        public string BusinessUnit
        {
            get { return _businessUnit; }
        }

    	public string NewUserName
    	{
			get { return _newUserName; }
    	}

		public string NewUserPassword
		{
			get { return _newUserPassword; }
		}

        public string SourceConnectionString
        {
            get
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
                sqlConnectionStringBuilder.DataSource = _sourceServer;
                sqlConnectionStringBuilder.InitialCatalog = _sourceDatabase;
                if (_useIntegratedSecurity)
                {
                    sqlConnectionStringBuilder.IntegratedSecurity = _useIntegratedSecurity;
                }
                else
                {
                    sqlConnectionStringBuilder.UserID = _sourceUserName;
                    sqlConnectionStringBuilder.Password = _sourcePassword;
                }
                return sqlConnectionStringBuilder.ConnectionString;
            }
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

        public CultureInfo CultureInfo
        {
            get { return _cultureInfo; }
        }

        public int DefaultResolution
        {
            get { return _defaultResolution; }
        }

        public bool UseIntegratedSecurity
        {
            get { return _useIntegratedSecurity; }
        }

        public bool OnlyRunMergeDefaultResolution
        {
            get { return _onlyRunMergeDefaultResolution; }
        }

        #endregion

        private void readArguments()
        {
            foreach (string s in _argumentCollection)
            {
                string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
                string switchValue = s.Remove(0, 3);
                
                switch (switchType)
                {
                    case "-SS":   // Source Server Name.
                        _sourceServer = switchValue;
                        break;
                    case "-SD":   // Source Database Name.
                        _sourceDatabase = switchValue;
                        break;
                    case "-SU":   // Source User Name.
                        _sourceUserName = switchValue;
                        break;
                    case "-SP":   // Source Password.
                        _sourcePassword = switchValue;
                        break;
                    case "-DS":   // Destination Server Name.
                        _destinationServer = switchValue;
                        break;
                    case "-DD":   // Destination Database Name.
                        _destinationDatabase = switchValue;
                        break;
                    case "-DU":   // Destination User Name.
                        _destinationUserName= switchValue;
                        break;
                    case "-DP":   // Destination Password.
                        _destinationPassword= switchValue;
                        break;
                    case "-TZ":   // TimeZone.
                        _timeZone = (TimeZoneInfo.FindSystemTimeZoneById(switchValue));
                        break;
                    case "-FD":   // Date From.
                        _fromDate = DateTime.Parse(switchValue, CultureInfo.CurrentCulture);
                        break;
                    case "-TD":  // Date To.
                        _toDate = DateTime.Parse(switchValue, CultureInfo.CurrentCulture);
                        break;
                    case "-BU":  // BusinessUnit Name.
                        _businessUnit = switchValue;
                        break;
                    case "-CU":  // Culture.
                        _cultureInfo = CultureInfo.GetCultureInfo(switchValue);
                        break;
                    case "-DR": // Force merge of Default Resolution to n.
                        _defaultResolution = Convert.ToInt32(switchValue, CultureInfo.CurrentCulture);
                        break;
                    case "-EE":
                        _useIntegratedSecurity = true;
                        break;
                    case "-OM": // Only run merge of default resolution.
                        _onlyRunMergeDefaultResolution = true;
                        break;
					case "-NA":
                		_newUserName = switchValue;
						break;
					case "-NP":
                		_newUserPassword = switchValue;
						break;
                }
            }
        }
    }
}