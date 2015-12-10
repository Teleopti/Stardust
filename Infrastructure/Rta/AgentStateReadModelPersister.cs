using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelPersister : IAgentStateReadModelPersister
	{
		private readonly CloudSafeSqlExecute executor = new CloudSafeSqlExecute();
		private readonly Func<SqlConnection> _connection;

		public AgentStateReadModelPersister(IConnectionStrings connectionStrings)
		{
			_connection = () =>
			{
				var conn = new SqlConnection(connectionStrings.Analytics());
				conn.Open();
				return conn;
			};
		}

		[InfoLog]
		public virtual void PersistActualAgentReadModel(AgentStateReadModel model)
		{
			executor.Run(_connection, connection =>
			{
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
				command.Parameters.Add("@StateStartTime", SqlDbType.DateTime).Value = model.StateStartTime;
				command.Parameters.Add("@NextStart", SqlDbType.DateTime).Value = model.NextStart;
				command.Parameters.Add("@AlarmStart", SqlDbType.DateTime).Value = model.AlarmStart;
				command.Parameters.Add("@Color", SqlDbType.Int).Value = model.Color;
				command.Parameters.Add("@StaffingEffect", SqlDbType.Float).Value = model.StaffingEffect;
				command.Parameters.Add("@Adherence", SqlDbType.Int).Value = model.Adherence ?? (object) DBNull.Value;
				command.Parameters.Add("@ReceivedTime", SqlDbType.DateTime).Value = model.ReceivedTime;
				command.Parameters.Add("@BusinessUnitId", SqlDbType.UniqueIdentifier).Value = model.BusinessUnitId;
				command.Parameters.Add("@SiteId", SqlDbType.UniqueIdentifier).Value = model.SiteId ?? (object) DBNull.Value;
				command.Parameters.Add("@TeamId", SqlDbType.UniqueIdentifier).Value = model.TeamId ?? (object) DBNull.Value;
				command.Parameters.Add("@BatchId", SqlDbType.DateTime).Value = model.BatchId ?? (object) DBNull.Value;
				command.Parameters.Add("@OriginalDataSourceId", SqlDbType.NVarChar).Value = model.OriginalDataSourceId ??
																							(object) DBNull.Value;

				command.ExecuteNonQuery();
			});
		}

		public void Delete(Guid personId)
		{
			executor.Run(_connection, connection =>
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText =
				"DELETE Rta.ActualAgentState " +
				"WHERE PersonId = @PersonId ";
				command.Parameters.Add("@PersonId", SqlDbType.UniqueIdentifier).Value = personId;
				
				command.ExecuteNonQuery();
			});
		}
	}
}