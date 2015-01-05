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
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CustomizeServerObject));

		private readonly Repository _repository;
		private readonly ServerConnectionInfo _analysisConnectionInfo;
		private readonly ServerConnectionInfo _sqlConnectionInfo;
		private readonly FolderInformation _folderInformation;
		
		private const string DataSourceViewName = "Teleopti Analytics";

		private const string dimensions = "02 Dimensions";
		private const string measures = "03 Measures";
		private const string calculatedMembers = "04 Calculated Members";


        public CustomizeServerObject(Repository repository, ServerConnectionInfo analysisConnectionInfo, ServerConnectionInfo sqlConnectionInfo, FolderInformation folderInformation)
        {
	        _repository = repository;
	        _analysisConnectionInfo = analysisConnectionInfo;
	        _sqlConnectionInfo = sqlConnectionInfo;
	        _folderInformation = folderInformation;
        }

		public void ApplyCustomization()
		{
			var folderOrFilePath = _folderInformation.CustomFilePath;
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

				forEachFileInSubFolder(folderOrFilePath + "\\" + dimensions);
				forEachFileInSubFolder(folderOrFilePath + "\\" + measures);
				forEachFileInCalculatedMembers(folderOrFilePath + "\\" + calculatedMembers);

			}
			else if (File.Exists(folderOrFilePath))
			{
				Logger.Info("\tRunning single custom script : " + _folderInformation.CustomFilePath);
				_repository.ExecuteAnyXmla(folderOrFilePath);
			}
			else
				Logger.Info("\tNo custom action");

		}

		private void forEachFileInSubFolder(string folder)
		{
			var cubeDimension = new CubeDimension(_analysisConnectionInfo);
			Array.ForEach(FileConstants.FilesOfTypeFromFolder(folder, FileConstants.Xmla).ToArray(), scriptFile =>
			{
				Logger.Info("\t" + scriptFile.Name);
				_repository.ExecuteAnyXmla(scriptFile.FullName);
				if (!folder.EndsWith(dimensions)) return;
				
				cubeDimension.AddByFileName(Path.GetFileNameWithoutExtension(scriptFile.Name));
			});
		}

		private void forEachFileInCalculatedMembers(string folder)
		{
			var parser = new ParseDataFromXml<CalculatedMemberDefinition>();
			Array.ForEach(FileConstants.FilesOfTypeFromFolder(folder, FileConstants.Xml).ToArray(), scriptFile =>
			{
				Logger.Info("\t" + scriptFile.Name);
				var calculatedMemberDefinition = parser.Parse(scriptFile.FullName);

				foreach (var calculatedMember in calculatedMemberDefinition.CalculatedMembers)
				{
					addCalculatedMeasure(calculatedMember);
				}
			});
		}

		private void addCalculatedMeasure(CalculatedMember calculatedMember)
		{
			var updater = new MdxScriptUpdater(_analysisConnectionInfo.ServerName);
			updater.MdxCommands.Add(calculatedMember.MdxString);
			updater.Update(_analysisConnectionInfo.DatabaseName, Repository.CubeName);
		}

		private void CreateDataSourceView(DatasourceViewDefinition datasourceViewDefinition)
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

		private void forEachTableInDatasourceview(RelationalTable table)
		{
			var adapter = new SqlDataAdapter(table.CommandText, _sqlConnectionInfo.ConnectionString);

			using (var server = new Server())
			{
				server.Connect(_analysisConnectionInfo.ConnectionString);
				using (var targetDb = server.Databases.GetByName(_analysisConnectionInfo.DatabaseName))
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
						AddRelation(tempDataSourceView, con);
					}
					tempDataSourceView.Update();
				}
			}
		}

		private void AddRelation(DataSourceView dsv, TeleoptiContraints constraint)
		{
			DataColumn fkColumn
				= dsv.Schema.Tables[constraint.FkTableName].Columns[constraint.FkColumName];
			DataColumn pkColumn
				= dsv.Schema.Tables[constraint.PkTableName].Columns[constraint.PkColumName];
			dsv.Schema.Relations.Add(
				string.Format(CultureInfo.InvariantCulture, "FK_{0}_{1}", constraint.FkTableName, constraint.FkColumName), pkColumn,
				fkColumn, true);
		}

		private bool verifyDatasourceView()
        {
			using (var server = new Server())
			{
				server.Connect(_analysisConnectionInfo.ConnectionString);
				using (var targetDb = server.Databases.GetByName(_analysisConnectionInfo.DatabaseName))
				{
					return targetDb.DataSourceViews.Contains(DataSourceViewName);
				}
			}
        }
	}
}