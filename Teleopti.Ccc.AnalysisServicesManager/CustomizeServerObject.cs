using System;
using System.Globalization;
using System.IO;
using System.Linq;
using log4net;
using Microsoft.AnalysisServices;
using System.Data;
using System.Data.SqlClient;
 
namespace AnalysisServicesManager
{
	public class CustomizeServerObject
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (CustomizeServerObject));

		private string _ASconnectionString;
		private string _databaseName;
		private string _SQLconnectionString;
		private const string _cubeName = "Teleopti Analytics";
		private const string DataSourceViewName = "Teleopti Analytics";

		private const string dimensions = "02 Dimensions";
		private const string measures = "03 Measures";
		private const string calculatedMembers = "04 Calculated Members";


        public CustomizeServerObject(CommandLineArgument argument)
        {
			_ASconnectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};", argument.AnalysisServer);
			_databaseName = argument.AnalysisDatabase;
			_SQLconnectionString = ExtractConnectionString.sqlConnectionStringSet(argument);
        }

		public void ApplyCustomization(CommandLineArgument argument)
		{
			var folderOrFilePath = argument.CustomFilePath;
			string dataSourceFile = folderOrFilePath + "\\01 Datasource\\DatasourceViewDefinition.xml";

			if(Directory.Exists(folderOrFilePath))
			{
				if (File.Exists(dataSourceFile))
				{
					Logger.Info("\tAdding custom data source view  ...");
					var parser = new ParseDataFromXml<DatasourceViewDefinition>();
					var tableDefinitionList = parser.Parse(dataSourceFile);
					CreateDataSourceView(tableDefinitionList);
				}

				ForEachFileInSubFolder(argument, folderOrFilePath + "\\" + dimensions);
				ForEachFileInSubFolder(argument, folderOrFilePath + "\\" + measures);
				ForEachFileInCalculatedMembers(argument, folderOrFilePath + "\\" + calculatedMembers);

			}
			else if (File.Exists(folderOrFilePath))
			{
				Logger.Info("\tRunning single custom script : " + argument.CustomFilePath);
				var repository = new Repository(argument);
				repository.ExecuteAnyXmla(argument, folderOrFilePath);
			}
			else
				Logger.Info("\tNo custom action");

		}

		public string NameOfFile(FileInfo file, string extension)
		{
			var name = file.Name.Replace("."+extension, "");
			var fileName = Convert.ToString(name);
			return fileName;
		}

		public void ForEachFileInSubFolder(CommandLineArgument argument, string folder)
		{
			if (!Directory.Exists(folder))
				return;

			var scriptsDirectoryInfo = new DirectoryInfo(folder);

			const string extension = "xmla";
			var scriptFiles = scriptsDirectoryInfo.GetFiles("*." + extension, SearchOption.TopDirectoryOnly);

			var applicableScriptFiles = from f in scriptFiles
										let name = NameOfFile(f, extension)
										orderby name
										select new { file = f };

			foreach (var scriptFile in applicableScriptFiles)
			{
				Logger.Info("\t" + scriptFile.file.Name);
				var repository = new Repository(argument);
				repository.ExecuteAnyXmla(argument, scriptFile.file.FullName);
				if (folder.EndsWith(dimensions))
				{
					AddCubeDimensionByFileName(Path.GetFileNameWithoutExtension(scriptFile.file.Name));
				}
			}
		}

		public void ForEachFileInCalculatedMembers(CommandLineArgument argument, string folder)
		{
			if(!Directory.Exists(folder))
				return;

			var scriptsDirectoryInfo = new DirectoryInfo(folder);

			const string extension = "xml";
			var scriptFiles = scriptsDirectoryInfo.GetFiles("*." + extension, SearchOption.TopDirectoryOnly);

			var applicableScriptFiles = from f in scriptFiles
										let name = NameOfFile(f, extension)
										orderby name
										select new { file = f };

			foreach (var scriptFile in applicableScriptFiles)
			{
				Logger.Info("\t" + scriptFile.file.Name);
				var parser = new ParseDataFromXml<CalculatedMemberDefinition>();
				var calculatedMemberDefinition = parser.Parse(scriptFile.file.FullName);

				foreach (var calculatedMember in calculatedMemberDefinition.CalculatedMembers)
				{
					addCalculatedMeasure(argument.AnalysisServer,argument.AnalysisDatabase, calculatedMember);
				}
			}
		}

		public void addCalculatedMeasure(string AsServer, string AsDatabase, CalculatedMember calculatedMember)
		{
			//add the member
			MdxScriptUpdater updater = new MdxScriptUpdater(AsServer);
			updater.MdxCommands.Add(calculatedMember.MdxString);
			updater.Update(AsDatabase, _cubeName);
		}

		public void AddCubeDimensionByFileName(string dimensionName)
		{
			using (var server = new Server())
			{
				server.Connect(_ASconnectionString);
				using (var targetDb = server.Databases.GetByName(_databaseName))
				{
					Cube cube = targetDb.Cubes.FindByName(_cubeName);

					Dimension dim = targetDb.Dimensions.GetByName(dimensionName);
					cube.Dimensions.Add(dim.Name, dim.Name, dim.Name);
					cube.Update(UpdateOptions.ExpandFull);
				}
			}
		}

		public void CreateDataSourceView(DatasourceViewDefinition datasourceViewDefinition)
		{
			if (!verifyDatasourceView())
			{
				throw new ArgumentException("Can't find Datasource View!");
			}

			foreach (var table in datasourceViewDefinition.DataTables)
			{
				forEachTableInDatasourceview(table);
			}
		}

		public void forEachTableInDatasourceview(RelationalTable table)
		{
			var adapter = new SqlDataAdapter(table.CommandText, _SQLconnectionString);

			using (var server = new Server())
			{
				server.Connect(_ASconnectionString);
				using (var targetDb = server.Databases.GetByName(_databaseName))
				{
					DataSourceView datasourceView = new DataSourceView();
					var tempDataSourceView = targetDb.DataSourceViews.FindByName(DataSourceViewName);
					DataTable[] dataTables = adapter.FillSchema(tempDataSourceView.Schema, SchemaType.Mapped, table.DbTableName);
					DataTable dataTable = dataTables[0];
					dataTable.ExtendedProperties.Add("TableType", table.TableType);
					dataTable.ExtendedProperties.Add("DbSchemaName", table.DbSchemaName);
					dataTable.ExtendedProperties.Add("DbTableName", table.DbTableName);
					dataTable.ExtendedProperties["DataSourceID"] = datasourceView.DataSourceID;
					dataTable.ExtendedProperties.Add("FriendlyName", table.DbTableName);
					foreach (var con in table.Constraints)
					{
						AddRelation(tempDataSourceView, con.FkTableName, con.FkColumName, con.PkTableName, con.PkColumName);
					}
					tempDataSourceView.Update();
				}
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
			using (var server = new Server())
			{
				server.Connect(_ASconnectionString);
				using (var targetDb = server.Databases.GetByName(_databaseName))
				{
					return targetDb.DataSourceViews.Contains(DataSourceViewName);
				}
			}
        }
	}
}