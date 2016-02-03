using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net;
using log4net.Repository.Hierarchy;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class WorkerNodeRepository:IWorkerNodeRepository
	{
		private readonly string _connectionString;
		private DataSet _jdDataSet;
		private DataTable _jdDataTable;

		public WorkerNodeRepository(string connectionString)
		{
			_connectionString = connectionString;
			initDS();
		}

		private void initDS()
		{
			_jdDataSet = new DataSet();
			_jdDataTable = new DataTable("[Stardust].WorkerNodes");
			_jdDataTable.Columns.Add(new DataColumn("Id", typeof(Guid)));
			_jdDataTable.Columns.Add(new DataColumn("Url", typeof(string)));
			_jdDataSet.Tables.Add(_jdDataTable);
		}

		public List<WorkerNode> LoadAll()
		{
			const string selectCommand = @"SELECT  Id ,Url FROM [Stardust].WorkerNodes";

			var listToReturn = new List<WorkerNode>();;
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand 
				{
					Connection = connection,
					CommandText = selectCommand,
					CommandType = CommandType.Text
				};
				connection.Open();
				var reader = command.ExecuteReader();
				if (reader.HasRows)                   
				{
					while (reader.Read())
					{
						var jobDefinition = new WorkerNode()
						{
							Id = (Guid)reader.GetValue(reader.GetOrdinal("Id")),
							Url = new Uri((string)reader.GetValue(reader.GetOrdinal("Url")))
						};
						listToReturn.Add(jobDefinition);
					}
				}
				reader.Close();
				connection.Close();
			}
			return listToReturn;
		}

		public List<WorkerNode> LoadAllFreeNodes()
		{
			const string selectCommand = @"SELECT * FROM [Stardust].WorkerNodes WHERE Url NOT IN (SELECT ISNULL(AssignedNode,'') FROM [Stardust].JobDefinitions)";

			var listToReturn = new List<WorkerNode>();
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					var command = new SqlCommand
					{
						Connection = connection,
						CommandText = selectCommand,
						CommandType = CommandType.Text
					};
					connection.Open();
					var reader = command.ExecuteReader();
					if (reader.HasRows)
					{
						while (reader.Read())
						{
							var jobDefinition = new WorkerNode()
							{
								Id = (Guid) reader.GetValue(reader.GetOrdinal("Id")),
								Url = new Uri((string) reader.GetValue(reader.GetOrdinal("Url")))
							};
							listToReturn.Add(jobDefinition);
						}
					}
					reader.Close();
					connection.Close();
				}
			}
			catch (TimeoutException exception)
			{
<<<<<<< HEAD
				Logger.Debug("Can not get WorkerNodes, maybe there is a lock in Stardust.JobDefinitions table", exception);
=======
                LogHelper.LogErrorWithLineNumber("Can not get WorkerNodes, maybe there is a lock in JobDefinitions table", exception);
>>>>>>> ad7ed1c63c136569d4d21c302709be670e2034ed
			}
			catch (Exception exception)
			{
                LogHelper.LogErrorWithLineNumber("Can not get WorkerNodes", exception);
			}

			return listToReturn;
		}
		public void Add(WorkerNode job)
		{
			var dr = _jdDataTable.NewRow();
			dr["Id"] = job.Id;
			dr["Url"] = job.Url.ToString();
			_jdDataTable.Rows.Add(dr);


			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var da = new SqlDataAdapter("Select * From [Stardust].WorkerNodes", connection);
				var builder = new SqlCommandBuilder(da);
				builder.GetInsertCommand();
				da.Update(_jdDataSet, "[Stardust].WorkerNodes");
				connection.Close();
			}
		}

		public void DeleteNode(Guid nodeId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var da = new SqlDataAdapter("Select * From [Stardust].WorkerNodes", connection);
				var command = new SqlCommand(
					"DELETE FROM [Stardust].WorkerNodes WHERE Id = @ID", connection);
				var parameter = command.Parameters.Add(
					"@ID", SqlDbType.UniqueIdentifier, 16, "Id");
				parameter.Value = nodeId;

				da.DeleteCommand = command;
				da.DeleteCommand.ExecuteNonQuery();
				connection.Close();
			}
		}
	}
}