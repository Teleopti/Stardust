using System.Globalization;

namespace AnalysisServicesManager
{
    public class CommandLineArgument
    {
	    private bool _useIntegratedSecurity;

	    private string _analysisServer;
	    private string _analysisDatabase;

		private string _sqlServer;
		private string _sqlDatabase;
		private string _sqlUser;
		private string _sqlPassword;

		private string _filePath;
		private string _customFilePath;
		private string _currentDir;

		public ServerConnectionInfo AnalysisConnectionInfo { private set; get; }
		public ServerConnectionInfo SqlConnectionInfo { private set; get; }
		public FolderInformation FolderInformation { get; private set; }

        public CommandLineArgument(string[] argumentCollection)
        {
            readArguments(argumentCollection);
            verifyArguments();
        }

        private void verifyArguments()
        {
            bool missingAsArguments = (_analysisServer != null || _analysisDatabase != null);

            bool missingSqlArguments = (_sqlServer != null || _sqlDatabase != null);

            bool missingSqlLogOn = (_sqlUser != null || _sqlPassword != null) || _useIntegratedSecurity;

            if (missingAsArguments || missingSqlArguments || missingSqlLogOn)
            {

            }
        }

        private void readArguments(string[] argumentCollection)
        {
			_currentDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
	        _customFilePath = _currentDir + @"\Custom";

            foreach (string s in argumentCollection)
            {
                string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
                string switchValue = s.Replace(switchType, string.Empty);

                switch (switchType)
                {
                    case "-AS":
                        _analysisServer = switchValue;
                        break;
                    case "-AD":
                        _analysisDatabase = switchValue;
                        break;
                    case "-SS":
                        _sqlServer = switchValue;
                        break;
                    case "-SD":
                        _sqlDatabase = switchValue;
                        break;
                    case "-SU":
                        _sqlUser = switchValue;
                        break;
                    case "-SP":
                        _sqlPassword = switchValue;
                        break;
                    case "-EE":
                        _useIntegratedSecurity = true;
                        break;
                    case "-FP":
                        _filePath = switchValue;
                        break;
                }
            }

	        AnalysisConnectionInfo = new ServerConnectionInfo(_analysisServer, _analysisDatabase, makeAnalysisConnectionString());
	        SqlConnectionInfo = new ServerConnectionInfo(_sqlServer, _sqlDatabase, makeSqlConnectionString());
	        FolderInformation = new FolderInformation(_currentDir, _filePath, _customFilePath);
        }

	    private string makeSqlConnectionString()
		{
			string sqlConnectionString;

			if (_useIntegratedSecurity)
			{
				sqlConnectionString =
				string.Format(CultureInfo.InvariantCulture,
							  "Application Name=TeleoptiPM;Data Source={0};Persist Security Info=True;Integrated Security=SSPI;Initial Catalog={1}",
							  _sqlServer, _sqlDatabase);
			}
			else
			{
				sqlConnectionString =
					string.Format(CultureInfo.InvariantCulture,
								  "Application Name=TeleoptiPM;Data Source={0};Persist Security Info=True;User ID={1};Password={2};Initial Catalog={3}",
								  _sqlServer, _sqlUser, _sqlPassword, _sqlDatabase);
			}
			return sqlConnectionString;
		}

	    private string makeAnalysisConnectionString()
	    {
		    return string.Format(CultureInfo.InvariantCulture, "Data Source={0};", _analysisServer);
	    }
    }
}