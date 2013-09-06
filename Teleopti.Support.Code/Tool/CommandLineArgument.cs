using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Support.Code.Tool
{
    public interface ICommandLineArgument
    {
        string AppServer { get; }
        string AppDatabase { get; }
        string AnalyticsDatabase { get; }
        string UserId { get; }
        string Password { get; }
        string BrokerUrl { get; }
        bool UseIntegratedSecurity { get; }
        string ConnectionString{ get; }
        bool ShowHelp { get; }
        string Help { get; }
        IList<SearchReplace> GetSearchReplaceList();
    }

    public class CommandLineArgument : ICommandLineArgument
    {
       private readonly string[] _argumentCollection;
        
        public CommandLineArgument(string[] argumentCollection)
        {
            AppServer = ".";
            _argumentCollection = argumentCollection;
            ReadArguments();
        }
        
        [SearchFor("$(DataSource)")]
        public string AppServer { get; private set; }
        public string AppDatabase { get; private set; }
        [SearchFor("$(AnalyticsDB)")]
        public string AnalyticsDatabase { get; private set; }
        public string UserId { get; private set; }
        public string Password { get; private set; }
        public string BrokerUrl { get; private set; }
        public bool UseIntegratedSecurity { get; private set; }
        public bool ShowHelp { get; private set; }

        public string Help
        {
            get { return @"Command line arguments:

        -? or ? or -HELP or HELP, Shows this help
        -DS is the Database Server, default '.' (local)
        -DB is the Application Database, TeleoptiCCC7 for example
        -AD is the AnalyticsDatabase, Teleopti_Analytics for example
        -US is the User Name used to log on to the Database
        -PW is the password used to log on to the Database
        -EE if present use Integrated Security to log on instead
        -BU is the Url to the Message Broker";
            }
        }

        public IList<SearchReplace> GetSearchReplaceList()
        {
            var ret = new List<SearchReplace>();
            foreach (var prop in (typeof(CommandLineArgument)).GetProperties())
            {
                foreach (var attribute in prop.GetCustomAttributes(true))
                {
                    var searchfor = (SearchForAttribute)attribute;
                    if (searchfor != null)
                    {
                        ret.Add(new SearchReplace { SearchFor = searchfor.SearchFor, ReplaceWith = prop.GetValue(this, null).ToString() });
                    }
                }
            }
            return ret;
        }

        public string ConnectionString
        {
            get
            {
                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
                    {
                        DataSource = AppServer,
                        InitialCatalog = AppDatabase
                    };

                if (UseIntegratedSecurity)
                {
                    sqlConnectionStringBuilder.IntegratedSecurity = UseIntegratedSecurity;
                }
                else
                {
                    sqlConnectionStringBuilder.UserID = UserId;
                    sqlConnectionStringBuilder.Password = Password;
                }
                return sqlConnectionStringBuilder.ConnectionString;
            }
        }

        

        private void ReadArguments()
        {
            foreach (string s in _argumentCollection)
            {
                switch (s)
                {
                    case "-?":
                    case "?":
                    case "-HELP":
                    case "HELP":
                        ShowHelp = true;
                        continue;
                }

                string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
                string switchValue = s.Remove(0, 3);

                switch (switchType)
                {
                    case "-DS":   // DB Server Name.
                        AppServer = switchValue;
                        break;
                    case "-DB":   // Database Name.
                        AppDatabase = switchValue;
                        break;
                    case "-AD":   // Database Name.
                        AnalyticsDatabase = switchValue;
                        break;
                    case "-US":   // User Name.
                        UserId = switchValue;
                        break;
                    case "-PW":   // Password.
                        Password = switchValue;
                        break;
                    case "-EE":
                        UseIntegratedSecurity = true;
                        break;
                    case "-BU":
                        BrokerUrl = switchValue;
                        break;
                    
                }
            }
        }
    }
}