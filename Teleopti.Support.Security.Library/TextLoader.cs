﻿using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Resources;
using log4net;

namespace Teleopti.Support.Security.Library
{
	public class TextLoader
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(TextLoader));

		public static int LoadAllTextsToDatabase(string connectionString)
		{
			var dataTable = new DataTable();
			AddColumnsToTable(dataTable);

			using (var connection = new SqlConnection())
			{
				// Get the ResourceManager
				Assembly a = Assembly.Load("Teleopti.Analytics.ReportTexts");
				var rm = new ResourceManager("Teleopti.Analytics.ReportTexts.Resources", a);

				//Get all cultures from config, or default
				string cultures = ConfigurationManager.AppSettings.Get("cultures");
				if (string.IsNullOrEmpty(cultures))
					cultures = "en;ar;cs;da;de;es;fi;fr;it;ja;ru;pt;pl;sk-SK;sq-AL;sv;th;tr;zh-CN";
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
			var rowsAffected = dataTable.Rows.Count;
			log.Debug("inserted " + rowsAffected + " report text rows.");
			return rowsAffected;
		}

		private static void doBulkCopy(string connectionString, DataTable dataTable)
		{
			using (var sqlBulkCopy = new SqlBulkCopy(connectionString))
			{
				sqlBulkCopy.DestinationTableName = "mart.language_translation";
				var timeout = 60;
				sqlBulkCopy.BulkCopyTimeout = timeout;
				
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
