using System.Collections.Generic;
using System.IO;
using System.Text;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Mapping;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class SqlServerProgrammabilityAuxiliary : IAuxiliaryDatabaseObject
	{
		public string SqlCreateString(Dialect dialect, IMapping p, string defaultCatalog, string defaultSchema)
		{
			return createSqlCreateString();
		}

		private static string createSqlCreateString()
		{
			var sql = new StringBuilder();
			string databaseDirectoryPath = findDatabaseDirectoryPathUsingBlackMagic();
			string _fileName;

			string progPath = databaseDirectoryPath + @"\TeleoptiCCC7\Programmability\";
			string[] directories = Directory.GetDirectories(progPath);

			//renaming fks amd similar
			sql.Append(
				File.ReadAllText(
					Path.Combine(progPath, @"Rename PK, FK.sql")));
			sql.Append("\nGO\n");

			//Add missing tables = Freemimum stuff
			sql.Append(
				File.ReadAllText(
					Path.Combine(progPath, @"NoneHiberateTables.sql")));
			sql.Append("\nGO\n");

			foreach (string directory in directories)
			{
				if (Directory.Exists(directory))
				{
					DirectoryInfo scriptsDirectoryInfo = new DirectoryInfo(directory);
					FileInfo[] scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

					foreach (FileInfo scriptFile in scriptFiles)
					{
						_fileName = scriptFile.FullName;

						using (TextReader textReader = new StreamReader(_fileName))
						{
							sql = sql.Append(textReader.ReadToEnd());
							sql.Append("\nGO\n");
						}
					}
				}
			}

			return sql.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SqlServerProgrammabilityAuxiliary")]
		private static string findDatabaseDirectoryPathUsingBlackMagic()
		{
			if (Directory.Exists(@"..\..\..\..\Database"))
			{
				return @"..\..\..\..\Database";
			}
			if (Directory.Exists(@"..\..\..\Database"))
			{
				return @"..\..\..\Database";
			}
			if (Directory.Exists(@"..\..\Database"))
			{
				return @"..\..\Database";
			}
			throw new FileNotFoundException("SqlServerProgrammabilityAuxiliary could not find the Database directory");
		}

		public string SqlDropString(Dialect dialect, string defaultCatalog, string defaultSchema)
		{
			return string.Empty;
		}

		public void AddDialectScope(string dialectName)
		{
		}

		public bool AppliesToDialect(Dialect dialect)
		{
			return true;
		}

		public void SetParameterValues(IDictionary<string, string> parameters)
		{
		}
	}
}