using System;
using System.Data.SqlClient;
using System.IO;
using log4net;
using Manager.IntegrationTest.Console.Host.Log4Net.Extensions;

namespace Manager.Integration.Test.Scripts
{
	public static class ScriptExecuteHelper
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (ScriptExecuteHelper));

		public static void ExecuteScript(string script,
		                                 string connectionstring)
		{
			if (string.IsNullOrEmpty(script))
			{
				throw new ArgumentException("script is empty.");
			}

			if (string.IsNullOrEmpty(connectionstring))
			{
				throw new ArgumentNullException("connectionstring");
			}

			using (var con = new SqlConnection(connectionstring))
			{
				con.Open();

				using (var command = new SqlCommand(script,
				                                    con))
				{
					try
					{
						command.ExecuteNonQuery();
					}

					catch (Exception exp)
					{
						Logger.ErrorWithLineNumber(exp.Message,
						                                 exp);
					}
				}

				con.Close();
			}
		}

		public static void ExecuteScriptFile(FileInfo scriptFile,
		                                     string connectionstring)
		{
			var script = File.ReadAllText(scriptFile.FullName);

			ExecuteScript(script,
			              connectionstring);
		}
	}
}