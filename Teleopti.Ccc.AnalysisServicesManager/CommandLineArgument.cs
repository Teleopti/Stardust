using System;
using System.Globalization;

namespace AnalysisServicesManager
{
    public class CommandLineArgument
    {
        public bool UseIntegratedSecurity { private set; get; }

        public string AnalysisServer { private set; get; }
        public string AnalysisDatabase { private set; get; }

        public string SqlServer { private set; get; }
        public string SqlDatabase { private set; get; }
        public string SqlUser { private set; get; }
        public string SqlPassword { private set; get; }

        public string FilePath { private set; get; }

		public string SQLconnectionString {set; get; }
		public string CustomFilePath { set; get; }

        public CommandLineArgument(string[] argumentCollection)
        {
            readArguments(argumentCollection);
            verifyArguments();
        }

        private void verifyArguments()
        {
            bool missingAsArguments = (AnalysisServer != null || AnalysisDatabase != null);

            bool missingSqlArguments = (SqlServer != null || SqlDatabase != null);

            bool missingSqlLogOn = (SqlUser != null || SqlPassword != null) || UseIntegratedSecurity;

            if (missingAsArguments || missingSqlArguments || missingSqlLogOn)
            {

            }
        }

        private void readArguments(string[] argumentCollection)
        {
            foreach (string s in argumentCollection)
            {
                string switchType = s.Substring(0, 3).ToUpper(CultureInfo.CurrentCulture);
                string switchValue = s.Replace(switchType, string.Empty);

                switch (switchType)
                {
                    case "-AS":
                        AnalysisServer = switchValue;
                        break;
                    case "-AD":
                        AnalysisDatabase = switchValue;
                        break;
                    case "-SS":
                        SqlServer = switchValue;
                        break;
                    case "-SD":
                        SqlDatabase = switchValue;
                        break;
                    case "-SU":
                        SqlUser = switchValue;
                        break;
                    case "-SP":
                        SqlPassword = switchValue;
                        break;
                    case "-EE":
                        UseIntegratedSecurity = true;
                        break;
                    case "-FP":
                        FilePath = switchValue;
                        break;
                }
            }
        }
    }
}