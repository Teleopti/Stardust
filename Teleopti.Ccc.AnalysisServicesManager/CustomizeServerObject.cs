using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.OleDb;
using Microsoft.AnalysisServices;
using System.Data;
using System.Data.SqlClient;
 
namespace AnalysisServicesManager
{
	public class RelationalTable
	{
		public string DbSchemaName { get; set; }
		public string DbTableName { get; set; }
		public string TableType { get; set; }
		public string CommandText { get; set; }
		public List<TeleoptiContraints> ListOfConstraints { get; set; }
	}

	public class TeleoptiContraints
	{
		public string FkTableName { get; set; }
		public string FkColumName { get; set; }
		public string PkTableName { get; set; }
		public string PkColumName { get; set; }
	}

	public class CustomizeServerObject
	{
		private string _ASconnectionString;
		private string _databaseName;
		private string _SQLconnectionString;
		private const string CubeName = "Teleopti Analytics";
		private const string DataSourceViewName = "Teleopti Analytics";

        public CustomizeServerObject(CommandLineArgument argument)
        {
			_ASconnectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};", argument.AnalysisServer);
			_databaseName = argument.AnalysisDatabase;
			_SQLconnectionString = ExtractConnectionString.sqlConnectionStringSet(argument);
        }

		public void ApplyCustomization(CommandLineArgument argument)
		{
			var folderName = argument.FilePath;

			var parser = new ParseDataViewInfoFromXml();
			var tableDefinitionList = parser.ExtractDataViewInfo(folderName + "\\01 Datasource\\DatasourceViewDefinition.xml");
			CreateDataSourceView(tableDefinitionList);

			var repository = new Repository(argument);
			argument.CustomFilePath = folderName + "\\02 MeasureGroups\\MeasureGroup.xmla";
			repository.ExecuteAnyXmla(argument);
			argument.CustomFilePath = folderName + "\\02 MeasureGroups\\MeasureGroup2.xmla";
			repository.ExecuteAnyXmla(argument);
		}

		public void CreateDataSourceView(IEnumerable<RelationalTable> tableDefinitionList)
		{
			if (!verifyDatasourceView())
			{
				throw new ArgumentException("Can't find Datasource View!");
			}

			foreach (var table in tableDefinitionList)
			{
				forEachTableInDatasourceview(table);
			}
		}

		public void forEachTableInDatasourceview(RelationalTable table)
		{
			var adapter = new SqlDataAdapter(table.CommandText, _SQLconnectionString);

			using (Server server = new Server())
			{
				server.Connect(_ASconnectionString);
				Database targetDb = server.Databases.GetByName(_databaseName);
				DataSourceView datasourceView = new DataSourceView();
				var tempDataSourceView = targetDb.DataSourceViews.FindByName(DataSourceViewName);
				DataTable[] dataTables = adapter.FillSchema(tempDataSourceView.Schema, SchemaType.Mapped, table.DbTableName);
				DataTable dataTable = dataTables[0];
				dataTable.ExtendedProperties.Add("TableType", table.TableType );
				dataTable.ExtendedProperties.Add("DbSchemaName", table.DbSchemaName );
				dataTable.ExtendedProperties.Add("DbTableName", table.DbTableName );
				dataTable.ExtendedProperties["DataSourceID"] = datasourceView.DataSourceID ;
				dataTable.ExtendedProperties.Add("FriendlyName", table.DbTableName);
				foreach(var con in table.ListOfConstraints )
				{
					AddRelation(tempDataSourceView, con.FkTableName , con.FkColumName , con.PkTableName , con.PkColumName );
				}
				tempDataSourceView.Update();
			}
		}

		private void AddRelation(DataSourceView dsv, String fkTableName, String fkColumnName, String pkTableName, String pkColumnName)
		{
			DataColumn fkColumn
				= dsv.Schema.Tables[fkTableName].Columns[fkColumnName];
			DataColumn pkColumn
				= dsv.Schema.Tables[pkTableName].Columns[pkColumnName];
			dsv.Schema.Relations.Add("FK_" + fkTableName + "_"
				+ fkColumnName, pkColumn, fkColumn, true);
		}

		public bool verifyDatasourceView()
        {
			using (Server server = new Server())
			{
				server.Connect(_ASconnectionString);
				Database targetDb = server.Databases.GetByName(_databaseName);

				DataSourceView datasourceView = new DataSourceView();
				if (targetDb.DataSourceViews.Contains(DataSourceViewName))
				{
					return true;
				}
				else { return false; }
			}
        }
	}
}