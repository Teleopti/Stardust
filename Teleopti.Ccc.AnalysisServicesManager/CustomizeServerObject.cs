using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.OleDb;
using System.IO;
using System.Linq;
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
		private const string _cubeName = "Teleopti Analytics";
		private const string DataSourceViewName = "Teleopti Analytics";

        public CustomizeServerObject(CommandLineArgument argument)
        {
			_ASconnectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};", argument.AnalysisServer);
			_databaseName = argument.AnalysisDatabase;
			_SQLconnectionString = ExtractConnectionString.sqlConnectionStringSet(argument);
        }

		public void ApplyCustomization(CommandLineArgument argument)
		{
			var folderOrFilePath = argument.CustomFilePath;
			if(Directory.Exists(folderOrFilePath))
			{
				Console.WriteLine("Running custom scripts from folder : " + argument.CustomFilePath);

				var parser = new ParseDataViewInfoFromXml();
				var tableDefinitionList = parser.ExtractDataViewInfo(folderOrFilePath + "\\01 Datasource\\DatasourceViewDefinition.xml");
				CreateDataSourceView(tableDefinitionList);

				ForEachFileInSubFolder(argument, folderOrFilePath + "\\02 Dimensions",true);
				ForEachFileInSubFolder(argument, folderOrFilePath + "\\03 Measures",false);
				ForEachFileInSubFolder(argument, folderOrFilePath + "\\04 Other",false);
			}
			else if (File.Exists(folderOrFilePath))
			{
				Console.WriteLine("Running single custom script : " + argument.CustomFilePath);
				var repository = new Repository(argument);
				repository.ExecuteAnyXmla(argument, folderOrFilePath);
			}
			else
				Console.WriteLine("No custom action");

		}

		public string NameOfFile(FileInfo file)
		{
			var name = file.Name.Replace(".xmla", "");
			var fileName = Convert.ToString(name);
			return fileName;
		}

		public void ForEachFileInSubFolder(CommandLineArgument argument, string folder, bool isDimension)
		{
			var scriptsDirectoryInfo = new DirectoryInfo(folder);

			var scriptFiles = scriptsDirectoryInfo.GetFiles("*.xmla", SearchOption.TopDirectoryOnly);

			var applicableScriptFiles = from f in scriptFiles
										let name = NameOfFile(f)
										orderby name
										select new { file = f };

			foreach (var scriptFile in applicableScriptFiles)
			{
				var repository = new Repository(argument);
				repository.ExecuteAnyXmla(argument, scriptFile.file.FullName);
				if (isDimension)
				{
					AddCubeDimensionByFileName(Path.GetFileNameWithoutExtension(scriptFile.file.Name));
				}
			}
		}

		public void AddCubeDimensionByFileName(string dimensionName)
		{
			using (Server server = new Server())
			{
				server.Connect(_ASconnectionString);
				Database targetDb = server.Databases.GetByName(_databaseName);
				Cube cube = targetDb.Cubes.FindByName(_cubeName);

				Dimension dim;
				dim = targetDb.Dimensions.GetByName(dimensionName);
				cube.Dimensions.Add(dim.Name, dim.Name, dim.Name);
				cube.Update(UpdateOptions.ExpandFull);
			}
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
				if (table.ListOfConstraints!=null)
				{
					foreach (var con in table.ListOfConstraints)
					{
						AddRelation(tempDataSourceView, con.FkTableName, con.FkColumName, con.PkTableName, con.PkColumName);
					}
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