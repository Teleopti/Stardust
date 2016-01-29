using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using log4net;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
	public class WorkerNodeRepository:IWorkerNodeRepository
	{
		private readonly string _connectionString;
		private DataSet _jdDataSet;
		private DataTable _jdDataTable;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(WorkerNodeRepository));

		public WorkerNodeRepository(string connectionString)
		{
			_connectionString = connectionString;
			initDS();
		}

		private void initDS()
		{
			_jdDataSet = new DataSet();
			_jdDataTable = new DataTable("WorkerNodes");
			_jdDataTable.Columns.Add(new DataColumn("Id", typeof(Guid)));
			_jdDataTable.Columns.Add(new DataColumn("Url", typeof(string)));
			_jdDataSet.Tables.Add(_jdDataTable);
		}

		public List<WorkerNode> LoadAll()
		{
			const string selectCommand = @"SELECT  Id ,Url FROM WorkerNodes";

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
			const string selectCommand = @"SELECT * FROM WorkerNodes WHERE Url NOT IN (SELECT ISNULL(AssignedNode,'') FROM JobDefinitions)";

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
				Logger.Debug("Can not get WorkerNodes, maybe there is a lock in JobDefinitions table", exception);
			}
			catch (Exception exception)
			{
				Logger.Debug("Can not get WorkerNodes", exception);
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
				var da = new SqlDataAdapter("Select * From WorkerNodes", connection);
				var builder = new SqlCommandBuilder(da);
				builder.GetInsertCommand();
				da.Update(_jdDataSet, "WorkerNodes");
				connection.Close();
			}
		}

		public void DeleteNode(Guid nodeId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var da = new SqlDataAdapter("Select * From WorkerNodes", connection);
				var command = new SqlCommand(
					"DELETE FROM WorkerNodes WHERE Id = @ID", connection);
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