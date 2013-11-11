using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Teleopti.Analytics.Portal.Utils
{
    public class TextLoader : MatrixBasePage
    {
        public static void LoadAllTextsToDatabase()
        {
            using (SqlConnection connection = new SqlConnection())
            {
                string insertSql = "INSERT INTO mart.language_translation VALUES ('{0}', {1}, '{2}',N'{3}', N'{4}') ";
                connection.ConnectionString = ConnectionString;
                // clear all first in mart.language_translation
                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = "DELETE FROM mart.language_translation";

                sqlCommand.Connection = connection;
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                sqlCommand.ExecuteNonQuery();

                // Get the RasourceManager
                Assembly a = Assembly.Load("Teleopti.Analytics.ReportTexts");
                ResourceManager rm = new ResourceManager("Teleopti.Analytics.ReportTexts.Resources", a);

                //Get all cultures from config
                string cultures = AppKeyReader.GetSetting("cultures");
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
							value = (string) resourceSet.Value;
						}
                        sqlCommand.CommandText = string.Format(insertSql, s, info.LCID, resourceSet.Key, value.Replace("'", "''"), englishText.Replace("'","''"));
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
