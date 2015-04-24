using System;
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
				

				// Get the RasourceManager
				Assembly a = Assembly.Load("Teleopti.Analytics.ReportTexts");
				var rm = new ResourceManager("Teleopti.Analytics.ReportTexts.Resources", a);

				//Get all cultures from config
				string cultures = ConfigurationManager.AppSettings.Get("cultures");
				string[] stringArray = cultures.Split(';');

				CultureInfo enlishInfo = CultureInfo.CreateSpecificCulture("en");
				var englishResorces = rm.GetResourceSet(enlishInfo, true, true);
				//for each culture and string load to database
				var dataTable = new DataTable();
				AddColumnsToTable(dataTable);
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
						//sqlCommand.CommandText = string.Format(insertSql, s, info.LCID, resourceSet.Key, value.Replace("'", "''"), englishText.Replace("'", "''"));
						//sqlCommand.ExecuteNonQuery();
						addRow(dataTable, info, resourceSet, englishText);
					}
				}
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
				doBulkCopy(connectionString, dataTable);
			}
		}

		private static void doBulkCopy(string connectionString, DataTable dataTable)
		{
			using (var sqlBulkCopy = new SqlBulkCopy(connectionString))
			{
				sqlBulkCopy.DestinationTableName = "mart.language_translation";
				//_sqlBulkCopy.NotifyAfter = 10000;
				var timeout = 60;
				sqlBulkCopy.BulkCopyTimeout = timeout;
				// Write from the source to the destination.
				sqlBulkCopy.WriteToServer(dataTable);
			}
		}

		private static void addRow(DataTable dataTable, CultureInfo info, DictionaryEntry resourceSet, string englishText)
		{
			DataRow dataRow = dataTable.NewRow();
			string value = "";
			if (resourceSet.Value != null)
			{
				value = (string)resourceSet.Value;
			}

			dataRow["Culture"] = info.Name;
			dataRow["language_id"] = info.LCID;
			dataRow["ResourceKey"] = resourceSet.Key;
			dataRow["term_language"] = value;
			dataRow["term_english"] = englishText; 

			dataTable.Rows.Add(dataRow);	
		}
		public static void AddColumnsToTable(DataTable table)
		{
			table.Columns.Add("Culture", typeof(string));
			table.Columns.Add("language_id", typeof(int));
			table.Columns.Add("ResourceKey", typeof(string));
			table.Columns.Add("term_language", typeof(string));
			table.Columns.Add("term_english", typeof(string));
		}
	}
}
