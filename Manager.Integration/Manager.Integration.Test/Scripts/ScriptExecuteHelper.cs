using System;
using System.Data.SqlClient;
using System.IO;
using Manager.Integration.Test.Helpers;

namespace Manager.Integration.Test.Scripts
{
    public static class ScriptExecuteHelper
    {
        public static void ExecuteScriptFile(FileInfo scriptFile,
                                             string connectionstring)
        {
            if (string.IsNullOrEmpty(connectionstring))
            {
                throw new ArgumentNullException("connectionstring");
            }

            string line;

            using (StreamReader sr = new StreamReader(scriptFile.FullName))
            {
                line = sr.ReadToEnd();
            }

            if (string.IsNullOrEmpty(line))
            {
                throw new ArgumentException("script file is empty.");
            }

            using (var con = new SqlConnection(connectionstring))
            {
                con.Open();

                using (SqlCommand command = new SqlCommand(line,con))
                {
                    try
                    {
                        int res = command.ExecuteNonQuery();
                    }

                    catch (Exception exp)
                    {
                        LogHelper.LogErrorWithLineNumber(exp.Message,exp);
                    }
                }

                con.Close();
            }
        }
    }
}