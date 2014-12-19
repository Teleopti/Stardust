using System;
using System.Collections.Generic;
using System.Globalization;
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

	public class CalculatedMember
	{
		public string MdxString { get; set; }
		public string AssociatedMeasureGroupID { get; set; }
		public string CalculationReference { get; set; }
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
					Console.WriteLine("\tAdding custom data source view  ...");
					var parser = new ParseDataViewInfoFromXml();
					var tableDefinitionList = parser.ExtractDataViewInfo(dataSourceFile);
					CreateDataSourceView(tableDefinitionList);
				}

				ForEachFileInSubFolder(argument, folderOrFilePath + "\\" + dimensions);
				ForEachFileInSubFolder(argument, folderOrFilePath + "\\" + measures);
				ForEachFileInCalculatedMembers(argument, folderOrFilePath + "\\" + calculatedMembers);

			}
			else if (File.Exists(folderOrFilePath))
			{
				Console.WriteLine("\tRunning single custom script : " + argument.CustomFilePath);
				var repository = new Repository(argument);
				repository.ExecuteAnyXmla(argument, folderOrFilePath);
			}
			else
				Console.WriteLine("\tNo custom action");

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
				Console.WriteLine("\t" + scriptFile.file.Name);
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
				Console.WriteLine("\t" + scriptFile.file.Name);
				var parser = new ParseCalculatedMemberInfoFromXml();
				var calculatedMemberList = parser.ExtractCalculatedMemberInfo(scriptFile.file.FullName);

				foreach (var calculatedMember in calculatedMemberList)
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

			//move the calculated member to correct Measure Group
			using (Server server = new Server())
			{
				server.Connect(_ASconnectionString);
				Database targetDb = server.Databases.GetByName(_databaseName);
				Cube cube = targetDb.Cubes.FindByName(_cubeName);
				MdxScript mdxScript = cube.MdxScripts["MdxScript"];

				if (!mdxScript.CalculationProperties.Contains(calculatedMember.CalculationReference))
				{
					CalculationProperty cp = new CalculationProperty(calculatedMember.CalculationReference, CalculationType.Member);
					cp.AssociatedMeasureGroupID = calculatedMember.AssociatedMeasureGroupID;
					cp.FormatString = "";
					cp.Visible = true;
					mdxScript.CalculationProperties.Add(cp);
					cube.Update(UpdateOptions.ExpandFull);
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

				if (targetDb.DataSourceViews.Contains(DataSourceViewName))
				{
					return true;
				}
				else { return false; }
			}
        }
	}
}