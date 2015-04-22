using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class TextLoader
	{
		public static void LoadAllTextsToDatabase(string connectionString)
		{
			using (var connection = new SqlConnection())
			{
				const string insertSql = "INSERT INTO mart.language_translation VALUES ('{0}', {1}, '{2}',N'{3}', N'{4}') ";
				connection.ConnectionString = connectionString;
				// clear all first in mart.language_translation
				var sqlCommand = new SqlCommand
				{
					CommandType = CommandType.Text,
					CommandText = "DELETE FROM mart.language_translation",
					Connection = connection
				};

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}
				sqlCommand.ExecuteNonQuery();

				// Get the RasourceManager
				Assembly a = Assembly.Load("Teleopti.Analytics.ReportTexts");
				var rm = new ResourceManager("Teleopti.Analytics.ReportTexts.Resources", a);

				//Get all cultures from config
				string cultures = ConfigurationManager.AppSettings.Get("cultures");
				string[] stringArray = cultures.Split(';');

				CultureInfo enlishInfo = CultureInfo.CreateSpecificCulture("en");
				var englishResorces = rm.GetResourceSet(enlishInfo, true, true);
				//for each culture and string load to database
				foreach (var s in stringArray)
				{
					CultureInfo info = CultureInfo.CreateSpecificCulture(s);
					var resorces = rm.GetResourceSet(info, true, true);
					foreach (DictionaryEntry resourceSet in resorces)
					{
						string englishText = englishResorces.GetString((string)resourceSet.Key) ?? "";
						string value = "";
						if (resourceSet.Value != null)
						{
							value = (string)resourceSet.Value;
						}
						sqlCommand.CommandText = string.Format(insertSql, s, info.LCID, resourceSet.Key, value.Replace("'", "''"), englishText.Replace("'", "''"));
						sqlCommand.ExecuteNonQuery();
					}
				}
			}
		}
	}
}
