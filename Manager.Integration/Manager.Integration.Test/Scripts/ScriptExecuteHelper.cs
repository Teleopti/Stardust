using System;
using System.Data.SqlClient;
using System.IO;
using log4net;
using Manager.Integration.Test.Helpers;

namespace Manager.Integration.Test.Scripts
{
    public static class ScriptExecuteHelper
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof (ScriptExecuteHelper));

        public static void ExecuteScriptFile(FileInfo scriptFile,
                                             string connectionstring)
        {
            if (string.IsNullOrEmpty(connectionstring))
            {
                throw new ArgumentNullException("connectionstring");
            }

            string line = File.ReadAllText(scriptFile.FullName);

            if (string.IsNullOrEmpty(line))
            {
                throw new ArgumentException("script file is empty.");
            }

            using (var con = new SqlConnection(connectionstring))
            {
                con.Open();

                using (SqlCommand command = new SqlCommand(line,
                                                           con))
                {
                    try
                    {
                        int res = command.ExecuteNonQuery();
                    }

                    catch (Exception exp)
                    {
                        LogHelper.LogErrorWithLineNumber(exp.Message,
                                                         Logger,
                                                         exp);
                    }

                }

                con.Close();
            }
        }
    }
}