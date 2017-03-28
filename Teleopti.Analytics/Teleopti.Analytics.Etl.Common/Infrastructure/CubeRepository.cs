using System.Globalization;
using Microsoft.AnalysisServices.AdomdClient;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class CubeRepository
	{
		private readonly string _aSserver;
		private readonly string _aSdatabaseName;
		private readonly int _defaultTimeZoneId;
		//private string _ASCubeName;

		public CubeRepository(string server, string databaseName, int defaultTimeZoneId)
		{
			_aSserver = server;
			_aSdatabaseName = databaseName;
			_defaultTimeZoneId = defaultTimeZoneId;
			//_ASCubeName = "Teleopti Analytics";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void Process()
		{

			var processXmla = string.Format(CultureInfo.InvariantCulture, "<Batch xmlns="http://schemas.microsoft.com/analysisservices/2003/engine"><ErrorConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2"><KeyErrorLimit>10</KeyErrorLimit><KeyErrorAction>DiscardRecord</KeyErrorAction></ErrorConfiguration><Parallel><Process xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\"><Object><DatabaseID>{0}</DatabaseID></Object><Type>ProcessFull</Type><WriteBackTableCreation>UseExisting</WriteBackTableCreation></Process></Parallel></Batch>", _aSdatabaseName);
			var setDefaultMembersXmla = string.Format(CultureInfo.InvariantCulture, "<Alter ObjectExpansion=\"ExpandFull\" xmlns=\"http://schemas.microsoft.com/analysisservices/2003/engine\"> <Object> <DatabaseID>{0}</DatabaseID> <DimensionID>Dim Time Zone</DimensionID> </Object> <ObjectDefinition> <Dimension xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ddl2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2\" xmlns:ddl2_2=\"http://schemas.microsoft.com/analysisservices/2003/engine/2/2\" xmlns:ddl100_100=\"http://schemas.microsoft.com/analysisservices/2008/engine/100/100\" xmlns:ddl200=\"http://schemas.microsoft.com/analysisservices/2010/engine/200\" xmlns:ddl200_200=\"http://schemas.microsoft.com/analysisservices/2010/engine/200/200\" xmlns:ddl300=\"http://schemas.microsoft.com/analysisservices/2011/engine/300\" xmlns:ddl300_300=\"http://schemas.microsoft.com/analysisservices/2011/engine/300/300\" xmlns:ddl400=\"http://schemas.microsoft.com/analysisservices/2012/engine/400\" xmlns:ddl400_400=\"http://schemas.microsoft.com/analysisservices/2012/engine/400/400\"> <ID>Dim Time Zone</ID> <Name>Time Zone</Name> <Source xsi:type=\"DataSourceViewBinding\"> <DataSourceViewID>Teleopti Analytics</DataSourceViewID> </Source> <Attributes> <Attribute> <ID>Dim Time Zone</ID> <Name>Time Zone</Name> <Usage>Key</Usage> <KeyColumns> <KeyColumn> <DataType>SmallInt</DataType> <Source xsi:type=\"ColumnBinding\"> <TableID>dbo_dim_time_zone</TableID> <ColumnID>time_zone_id</ColumnID> </Source> </KeyColumn> </KeyColumns> <NameColumn> <DataType>WChar</DataType> <DataSize>100</DataSize> <Source xsi:type=\"ColumnBinding\"> <TableID>dbo_dim_time_zone</TableID> <ColumnID>time_zone_name</ColumnID> </Source> </NameColumn> <DefaultMember>[Time Zone].[Time Zone].&amp;[{1}]</DefaultMember> <IsAggregatable>false</IsAggregatable> </Attribute> </Attributes> </Dimension> </ObjectDefinition></Alter>", _aSdatabaseName, _defaultTimeZoneId);
			var connectionString = string.Format(CultureInfo.InvariantCulture, "Data Source={0};", _aSserver);

			using (var connection = new AdomdConnection(connectionString))
			{

				connection.Open();
				connection.ChangeDatabase(_aSdatabaseName);

				using (AdomdCommand cmd = connection.CreateCommand())
				{
					if (connection.Cubes.Count == 0)
					{
						//if never processed, do it once else "setDefaultMembersMDX" will crash
						executeCubeQuery(cmd, processXmla);
					}

					executeCubeQuery(cmd, setDefaultMembersXmla);
					executeCubeQuery(cmd, processXmla);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private static void executeCubeQuery(AdomdCommand cmd, string cmdText)
		{
			cmd.CommandText = cmdText;
			cmd.ExecuteNonQuery();
		}
	}
}
