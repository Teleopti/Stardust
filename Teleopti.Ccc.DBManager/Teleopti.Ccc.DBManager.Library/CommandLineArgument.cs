using System;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Ccc.DBManager.Library
{
    public class CommandLineArgument
    {
    	private string _applicationName = "Teleopti.Ccc.DBManager";
        private string _serverName = ".";
        private string _databaseName;
        private string _userName;
        private string _password;
        private string _appUserName = "";
        private string _appPwd = "";
        private bool _permissionMode;
        private string _currentLanguage = "us_english";
        private int _targetBuildNumber;
        private bool _useIntegratedSecurity;
        private bool _isWindowsGroupName;
        private DatabaseType _targetDatabaseType;
        private bool _willCreateNewDatabase;
        private string _businessUnitName;
        private bool _patchMode;

        public CommandLineArgument(string[] args)
        {
            foreach (string s in args)
            {
                string switchType = s.Substring(0, 2).ToUpper(CultureInfo.CurrentCulture);
                string switchValue = s.Remove(0,2);
                switch(switchType)
                {
                    case "-S" :
                        _serverName = switchValue;
                        break;
                    case "-D" :
                        _databaseName = switchValue;
                        break;
                    case "-U":
                        _userName = switchValue;
                        break;
                    case "-P":
                        _password = switchValue;
                        break;
                    case "-N":
                        _targetBuildNumber = Convert.ToInt32(switchValue);
                        break;
                    case "-E":
                        _useIntegratedSecurity = true;
                        break;
                    case "-O":
                        _targetDatabaseType = (DatabaseType) Enum.Parse(typeof(DatabaseType), switchValue);
                        break;
                    case "-C":
                        _willCreateNewDatabase = true;
                        _patchMode = true;
                        break;
                    case "-B":
                        _businessUnitName = switchValue;
                        break;
                    case "-T":
                        _patchMode = true;
                        break;
                    case "-L":
						_permissionMode = true;
                        string[] userpwd = switchValue.Split(':');
                        if (userpwd.Length == 2)
                        {
                            _appUserName = userpwd[0];
                            _appPwd = userpwd[1];
                            _isWindowsGroupName = false;
                        }
                        else
                        {
                            throw new Exception("Not the correct inparameters for application sql user and password");
                        }
                        break;
                    case "-W":
						_permissionMode = true;
                        _appUserName = switchValue;
                        _isWindowsGroupName = true;
                        break;
                    //case "-I":
                    //    _schemaName = switchValue;
                    //    break;
                    case "-F":
                        PathToDbManager = switchValue;
                        break;
                    case "-R":
                        _permissionMode = true;
                        break;
                }
                
            }
        }

	    public string PathToDbManager { get; set; }

	    public string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
                sqlConnectionStringBuilder.DataSource = _serverName;
                sqlConnectionStringBuilder.InitialCatalog = _databaseName;
                sqlConnectionStringBuilder.ApplicationName = _applicationName;
                sqlConnectionStringBuilder.CurrentLanguage = _currentLanguage;
                if (_useIntegratedSecurity)
                {
                    sqlConnectionStringBuilder.IntegratedSecurity = _useIntegratedSecurity;
                
                }
                else
                {
                    sqlConnectionStringBuilder.UserID = _userName;
                    sqlConnectionStringBuilder.Password = _password;
                }
                return sqlConnectionStringBuilder.ConnectionString;
            }
        }

        public string ConnectionStringToMaster
        {
            get
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
                sqlConnectionStringBuilder.DataSource = _serverName;
                sqlConnectionStringBuilder.InitialCatalog = DatabaseHelper.MasterDatabaseName;
                sqlConnectionStringBuilder.CurrentLanguage = _currentLanguage;
                sqlConnectionStringBuilder.ApplicationName = _applicationName;
                if (_useIntegratedSecurity)
                {
                    sqlConnectionStringBuilder.IntegratedSecurity = _useIntegratedSecurity;
                }
                else
                {
                    sqlConnectionStringBuilder.UserID = _userName;
                    sqlConnectionStringBuilder.Password = _password;
                
                }
                
                
                return sqlConnectionStringBuilder.ConnectionString;
            }
        }

		public string ConnectionStringAppLogOn (string databaseName)
		{
			{
				SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
				sqlConnectionStringBuilder.DataSource = _serverName;
				sqlConnectionStringBuilder.InitialCatalog = databaseName;
				sqlConnectionStringBuilder.CurrentLanguage = _currentLanguage;
				sqlConnectionStringBuilder.ApplicationName = _applicationName;
				if (_useIntegratedSecurity)
				{
					sqlConnectionStringBuilder.IntegratedSecurity = _useIntegratedSecurity;
				}
				else
				{
					sqlConnectionStringBuilder.UserID = _appUserName;
					sqlConnectionStringBuilder.Password = _appPwd;
				}
				return sqlConnectionStringBuilder.ConnectionString;
			}
		}
        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        /// <value>The name of the server.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-10
        /// </remarks>
        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>The name of the database.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-10
        /// </remarks>
        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-10
        /// </remarks>
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the user or windows group tha application will use when running 
        /// </summary>
        /// <value>The name of the application sql user or windows group.</value>
        /// <remarks>
        /// Created by: roberts
        /// Created date: 2008-10-21
        /// </remarks>
        public string appUserName
        {
            get { return _appUserName; }
            set { _appUserName = value; }
        }

        /// <summary>
        /// Gets or sets the password for the sql user if one is assigned
        /// </summary>
        /// <value>The password of the application sql user or windows group.</value>
        /// <remarks>
        /// Created by: roberts
        /// Created date: 2008-10-21
        /// </remarks>
        public string appUserPwd
        {
            get { return _appPwd; }
            set { _appPwd = value; }
        }

        /// <summary>
        /// returns true if appUserName is a window group name
        /// </summary>
        /// <remarks>
        /// Created by: roberts
        /// Created date: 2008-10-21
        /// </remarks>
        public bool isWindowsGroupName
        {
            get { return _isWindowsGroupName; }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-10
        /// </remarks>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Gets or sets the target build number.
        /// </summary>
        /// <value>The target build number.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-10
        /// </remarks>
        public int TargetBuildNumber
        {
            get { return _targetBuildNumber; }
            set { _targetBuildNumber = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use integrated security].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [use integrated security]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-10
        /// </remarks>
        public bool UseIntegratedSecurity
        {
            get { return _useIntegratedSecurity; }
            set { _useIntegratedSecurity = value; }
        }

        /// <summary>
        /// Gets or sets the type of the target database.
        /// </summary>
        /// <value>The type of the target database.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-10
        /// </remarks>
        public DatabaseType TargetDatabaseType
        {
            get { return _targetDatabaseType; }
            set { _targetDatabaseType = value; }
        }

        /// <summary>
        /// Gets the name of the target database type.
        /// </summary>
        /// <value>The name of the target database type.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-19
        /// </remarks>
        public string TargetDatabaseTypeName
        {
            get
            {
				return _targetDatabaseType.GetName();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [will create new database].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [will create new database]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-11
        /// </remarks>
        public bool WillCreateNewDatabase
        {
            get { return _willCreateNewDatabase; }
            set { _willCreateNewDatabase = value; }
        }

        /// <summary>
        /// Gets or sets the name of the business unit.
        /// </summary>
        /// <value>The name of the business unit.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-11
        /// </remarks>
        public string BusinessUnitName
        {
            get { return _businessUnitName; }
            set { _businessUnitName = value; }
        }

        public bool PermissionMode
        {
            get { return _permissionMode; }
            set { _permissionMode = value; }
        }

        public bool PatchMode
        {
            get { return _patchMode; }
            set { _patchMode = value; }
        }

        //public string SchemaName
        //{
        //    get { return _schemaName; }
        //}
    }
}
