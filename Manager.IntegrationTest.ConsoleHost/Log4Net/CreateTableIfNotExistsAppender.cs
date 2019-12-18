using System;
using System.Configuration;
using System.Data.SqlClient;
using log4net.Appender;

namespace Manager.IntegrationTest.ConsoleHost.Log4Net
{
	public class AdoNetCreateTableIfNotExistsAppender : AdoNetAppender
	{
		private const string CreateTableScript =
			@"CREATE TABLE Stardust.Logging (
				[Id] [int] IDENTITY (1, 1) NOT NULL,
				[Date] [datetime] NOT NULL,
				[Thread] [varchar] (255) NOT NULL,
				[Level] [varchar] (50) NOT NULL,
				[Logger] [varchar] (255) NOT NULL,
				[Message] [varchar] (4000) NOT NULL,
				[Exception] [varchar] (2000) NULL)";


		public override void ActivateOptions()
		{
			CreateTableIfNotExists();
		}

		private void CreateTableIfNotExists()
		{
			try
			{
				if (!string.IsNullOrEmpty(base.ConnectionStringName))
				{
					var configurationManager =
						ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

					var connectionString =
						configurationManager.ConnectionStrings.ConnectionStrings[base.ConnectionStringName];

					using (SqlConnection connection = new SqlConnection(connectionString.ConnectionString))
					{
						connection.Open();

						using (SqlCommand sqlCommand = new SqlCommand(CreateTableScript, connection))
						{
							sqlCommand.CommandText = CreateTableScript;

							sqlCommand.ExecuteNonQuery();
						}

						connection.Close();
					}
				}
			}

			catch (Exception)
			{
				// Do nothing.
			}

		}
	}
}