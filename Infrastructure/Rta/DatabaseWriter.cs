using System;
using System.Data;
using log4net;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseWriter : IDatabaseWriter
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(DatabaseWriter));

		public DatabaseWriter(IDatabaseConnectionFactory databaseConnectionFactory,
							  IDatabaseConnectionStringHandler databaseConnectionStringHandler)
		{
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseConnectionStringHandler = databaseConnectionStringHandler;
		}

		public StateCodeInfo AddAndGetStateCode(string stateCode, string stateDescription, Guid platformTypeId, Guid businessUnit)
		{
			var stateId = Guid.NewGuid();
			var defaultStateGroupId = Guid.Empty;
			var defaultStateGroupName = "";
			string getDefaultStateGroupQuery = string.Format(@"SELECT Name, Id, BusinessUnit FROM RtaStateGroup WHERE DefaultStateGroup = 1 AND BusinessUnit = '{0}'", businessUnit);
			const string insert = @"INSERT INTO RtaState (Id, Name, StateCode, PlatformTypeId, Parent) VALUES ('{0}', N'{4}', N'{1}', '{2}', '{3}')";

			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = getDefaultStateGroupQuery;
				connection.Open();
				string insertStatement = string.Empty;
				var reader = command.ExecuteReader();
				while (reader.Read())
				{
					defaultStateGroupId = reader.GetGuid(reader.GetOrdinal("Id"));
					defaultStateGroupName = reader.GetString(reader.GetOrdinal("Name"));

					insertStatement = string.Format(insert, stateId, stateCode, platformTypeId, defaultStateGroupId, stateDescription);
				}
				reader.Close();

				if (!string.IsNullOrEmpty(insertStatement) && !string.IsNullOrEmpty(stateCode))
				{
					command = connection.CreateCommand();
					command.CommandText = insertStatement;
					command.ExecuteNonQuery();
				}
			}
			return new StateCodeInfo
				{
					StateGroupId = defaultStateGroupId,
					StateGroupName = defaultStateGroupName,
					BusinessUnitId = businessUnit,
					StateName = stateDescription,
					PlatformTypeId = platformTypeId,
					StateCode = stateCode,
				};
		}

		public void PersistActualAgentReadModel(AgentStateReadModel model)
		{
			using (
				var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "[RTA].[rta_addorupdate_actualagentstate]";

				command.Parameters.Add("@PersonId", SqlDbType.UniqueIdentifier).Value = model.PersonId;
				command.Parameters.Add("@StateCode", SqlDbType.NVarChar).Value = model.StateCode;
				command.Parameters.Add("@PlatformTypeId", SqlDbType.UniqueIdentifier).Value = model.PlatformTypeId;
				command.Parameters.Add("@State", SqlDbType.NVarChar).Value = model.State;
				command.Parameters.Add("@AlarmName", SqlDbType.NVarChar).Value = model.AlarmName;
				command.Parameters.Add("@StateId", SqlDbType.UniqueIdentifier).Value = model.StateId;
				command.Parameters.Add("@Scheduled", SqlDbType.NVarChar).Value = model.Scheduled;
				command.Parameters.Add("@ScheduledId", SqlDbType.UniqueIdentifier).Value = model.ScheduledId;
				command.Parameters.Add("@AlarmId", SqlDbType.UniqueIdentifier).Value = model.AlarmId;
				command.Parameters.Add("@ScheduledNext", SqlDbType.NVarChar).Value = model.ScheduledNext;
				command.Parameters.Add("@ScheduledNextId", SqlDbType.UniqueIdentifier).Value = model.ScheduledNextId;
				command.Parameters.Add("@StateStart", SqlDbType.DateTime).Value = model.StateStart;
				command.Parameters.Add("@NextStart", SqlDbType.DateTime).Value = model.NextStart;
				command.Parameters.Add("@AlarmStart", SqlDbType.DateTime).Value = model.AlarmStart;
				command.Parameters.Add("@Color", SqlDbType.Int).Value = model.Color;
				command.Parameters.Add("@StaffingEffect", SqlDbType.Float).Value = model.StaffingEffect;
				command.Parameters.Add("@ReceivedTime", SqlDbType.DateTime).Value = model.ReceivedTime;
				command.Parameters.Add("@BusinessUnitId", SqlDbType.UniqueIdentifier).Value = model.BusinessUnitId;
				command.Parameters.Add("@SiteId", SqlDbType.UniqueIdentifier).Value = model.SiteId ?? (object)DBNull.Value;
				command.Parameters.Add("@TeamId", SqlDbType.UniqueIdentifier).Value = model.TeamId ?? (object)DBNull.Value;
				command.Parameters.Add("@BatchId", SqlDbType.DateTime).Value = model.BatchId ?? (object)DBNull.Value;
				command.Parameters.Add("@OriginalDataSourceId", SqlDbType.NVarChar).Value = model.OriginalDataSourceId ?? (object)DBNull.Value;

				command.ExecuteNonQuery();
				LoggingSvc.DebugFormat("Saved state: {0} to database", model);
			}
		}

	}
}