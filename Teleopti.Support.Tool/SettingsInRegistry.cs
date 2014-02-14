using System;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Teleopti.Support.Tool
{
    public class SettingsInRegistry
    {
        private const string SqlServerNameValueName = "SQLServerName";
        private const string SqlUserNameValueName = "SQLUserName";
        private const string UseWindowsAuthenticationValueName = "UseWindowsAuthentication";
        private string _sqlServerName;
        private string _sqlUserName;
        private bool _useWindowsAuthentication;
        private readonly string _keyName;

        public SettingsInRegistry()
        {
            _keyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\SupportToolSettings";


            object registryValue = Registry.GetValue(_keyName, UseWindowsAuthenticationValueName, @"False");
            if (registryValue == null)
            {
                registryValue = "false";
            }
            _useWindowsAuthentication = bool.Parse((string)registryValue);


            _sqlServerName = (string)Registry.GetValue(_keyName, SqlServerNameValueName, @"");
            if (string.IsNullOrEmpty(_sqlServerName))
            {
                _sqlServerName = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Teleopti\TeleoptiCCC\InstallationSettings", "SQL_SERVER_NAME", @"");
            }

            _sqlUserName = (string)Registry.GetValue(_keyName, SqlUserNameValueName, @"");
        }

        public string SqlServerName
        {
            get {
                return _sqlServerName;
            }
            set {
                _sqlServerName = value;
            }
        }

        public string SqlUserName
        {
            get {
                return _sqlUserName;
            }
            set {
                _sqlUserName = value;
            }
        }

        public bool UseWindowsAuthentication
        {
            get {
                return _useWindowsAuthentication;
            }
            set {
                _useWindowsAuthentication = value;
            }
        }

        public void SaveAll()
        {
            if (_sqlServerName == null) { _sqlServerName = string.Empty; }
            if (_sqlUserName == null) { _sqlUserName= string.Empty; }
			  try
           {
				  Registry.SetValue(_keyName, SqlServerNameValueName, _sqlServerName);
				  Registry.SetValue(_keyName, SqlUserNameValueName, _sqlUserName);
				  Registry.SetValue(_keyName, UseWindowsAuthenticationValueName, _useWindowsAuthentication.ToString(CultureInfo.InvariantCulture));
	        }
	        catch (Exception exception)
	        {
		        MessageBox.Show(exception.Message);
	        }
        }
    }
}