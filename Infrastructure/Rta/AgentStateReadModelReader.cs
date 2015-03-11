using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof (IDatabaseReader));

		public AgentStateReadModelReader(
			IDatabaseConnectionFactory databaseConnectionFactory,
			IDatabaseConnectionStringHandler databaseConnectionStringHandler
			)
		{
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseConnectionStringHandler = databaseConnectionStringHandler;
		}

		public IList<AgentStateReadModel> Load(IEnumerable<IPerson> persons)
        {
            var guids = persons.Select(person => person.Id.GetValueOrDefault()).ToList();
	        return Load(guids);
        }

        public IList<AgentStateReadModel> Load(IEnumerable<Guid> personGuids)
        {
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                var ret = new List<AgentStateReadModel>();
				foreach (var personList in personGuids.Batch(400))
				{
					ret.AddRange(((NHibernateStatelessUnitOfWork) uow).Session.CreateSQLQuery(
						@"SELECT 
							PlatformTypeId,
							BusinessUnitId,
							StateCode,
							StateId,
							State,
							ScheduledId,
							Scheduled,
							ScheduledNextId,
							ScheduledNext,
							ReceivedTime,
							Color,
							AlarmId,
							AlarmName,
							StateStart,
							NextStart,
							BatchId,
							OriginalDataSourceId,
							AlarmStart,
							PersonId,
							StaffingEffect,
							Adherence,
							TeamId,
							SiteId 
						FROM RTA.ActualAgentState WITH (NOLOCK) WHERE PersonId IN(:persons)")
						.SetParameterList("persons", personList)
						.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
						.SetReadOnly(true)
						.List<AgentStateReadModel>());
				}
                return ret;
            }
		}

	    public IList<AgentStateReadModel> LoadForTeam(Guid teamId)
	    {
			using (var uow = StatisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					@"SELECT 
						PlatformTypeId,
						BusinessUnitId,
						StateCode,
						StateId,
						State,
						ScheduledId,
						Scheduled,
						ScheduledNextId,
						ScheduledNext,
						ReceivedTime,
						Color,
						AlarmId,
						AlarmName,
						StateStart,
						NextStart,
						BatchId,
						OriginalDataSourceId,
						AlarmStart,
						PersonId,
						StaffingEffect,
						Adherence,
						TeamId,
						SiteId 
					FROM RTA.ActualAgentState WITH (NOLOCK) WHERE TeamId = :teamId")
					.SetParameter("teamId", teamId)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}  
	    }

	    private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
            return identity.DataSource.Statistic;
        }








		public AgentStateReadModel GetCurrentActualAgentState(Guid personId)
		{
			LoggingSvc.DebugFormat("Getting old state for person: {0}", personId);

			var agentState = queryActualAgentStates(personId).FirstOrDefault();

			if (agentState == null)
				LoggingSvc.DebugFormat("Found no state for person: {0}", personId);
			else
				LoggingSvc.DebugFormat("Found old state for person: {0}, AgentState: {1}", personId, agentState);

			return agentState;
		}

		public IEnumerable<AgentStateReadModel> GetActualAgentStates()
		{
			return queryActualAgentStates(null);
		}

		private IEnumerable<AgentStateReadModel> queryActualAgentStates(Guid? personId)
		{
			var query = "SELECT * FROM RTA.ActualAgentState";
			if (personId.HasValue)
				query = string.Format("SELECT * FROM RTA.ActualAgentState WHERE PersonId ='{0}'", personId);
			using (
				var connection =
					_databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = query;
				connection.Open();
				using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (reader.Read())
					{
						yield return new AgentStateReadModel
						{
							PlatformTypeId = reader.Guid("PlatformTypeId"),
							BusinessUnitId = reader.NullableGuid("BusinessUnitId") ?? Guid.Empty,
							StateCode = reader.String("StateCode"),
							StateId = reader.NullableGuid("StateId"),
							State = reader.String("State"),
							ScheduledId = reader.NullableGuid("ScheduledId"),
							Scheduled = reader.String("Scheduled"),
							ScheduledNextId = reader.NullableGuid("ScheduledNextId"),
							ScheduledNext = reader.String("ScheduledNext"),
							ReceivedTime = reader.DateTime("ReceivedTime"),
							Color = reader.NullableInt("Color"),
							AlarmId = reader.NullableGuid("AlarmId"),
							AlarmName = reader.String("AlarmName"),
							StateStart = reader.NullableDateTime("StateStart"),
							NextStart = reader.NullableDateTime("NextStart"),
							BatchId = reader.NullableDateTime("BatchId"),
							OriginalDataSourceId = reader.String("OriginalDataSourceId"),
							AlarmStart = reader.NullableDateTime("AlarmStart"),
							PersonId = reader.Guid("PersonId"),
							StaffingEffect = reader.NullableDouble("StaffingEffect"),
							Adherence = reader.NullableInt("Adherence"),
							TeamId = reader.NullableGuid("TeamId"),
							SiteId = reader.NullableGuid("SiteId")
						};
					}
				}
			}
		}

		public IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			var missingUsers = new List<AgentStateReadModel>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "[RTA].[rta_get_last_batch]";
				command.Parameters.Add(new SqlParameter("@datasource_id", dataSourceId));
				command.Parameters.Add(new SqlParameter("@batch_id", batchId));

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					missingUsers.Add(new AgentStateReadModel
					{
						BusinessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId")),
						PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
						StateCode = reader.String("StateCode"),
						PlatformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId")),
						State = reader.String("State"),
						StateId = reader.NullableGuid("StateId"),
						Scheduled = reader.String("Scheduled"),
						ScheduledId = reader.NullableGuid("ScheduledId"),
						StateStart = reader.NullableDateTime("StateStart"),
						ScheduledNext = reader.String("ScheduledNext"),
						ScheduledNextId = reader.NullableGuid("ScheduledNextId"),
						NextStart = reader.NullableDateTime("NextStart"),
					});
				}
			}

			return missingUsers;
		}

    }
}
