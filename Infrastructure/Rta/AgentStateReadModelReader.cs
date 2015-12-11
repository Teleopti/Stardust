using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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
		private readonly IConnectionStrings _connectionStrings;

		public AgentStateReadModelReader(
			IConnectionStrings connectionStrings
			)
		{
			_connectionStrings = connectionStrings;
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
							StateStartTime,
							NextStart,
							BatchId,
							OriginalDataSourceId,
							AdherenceStartTime,
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
						StateStartTime,
						NextStart,
						BatchId,
						OriginalDataSourceId,
						AdherenceStartTime,
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

		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds)
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
						StateStartTime,
						NextStart,
						BatchId,
						OriginalDataSourceId,
						AdherenceStartTime,
						PersonId,
						StaffingEffect,
						Adherence,
						TeamId,
						SiteId 
					FROM RTA.ActualAgentState WITH (NOLOCK) WHERE SiteId IN (:siteIds)")
					.SetParameterList("siteIds", siteIds)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds)
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
						StateStartTime,
						NextStart,
						BatchId,
						OriginalDataSourceId,
						AdherenceStartTime,
						PersonId,
						StaffingEffect,
						Adherence,
						TeamId,
						SiteId 
					FROM RTA.ActualAgentState WITH (NOLOCK) WHERE TeamId IN (:teamIds)")
					.SetParameterList("teamIds", teamIds)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
		}

		private IUnitOfWorkFactory StatisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
            return identity.DataSource.Statistic;
        }

		public IEnumerable<AgentStateReadModel> GetActualAgentStates()
		{
			return queryActualAgentStates2("SELECT * FROM Rta.ActualAgentState", new parameters[] { });
		}

		[InfoLog]
		public virtual AgentStateReadModel GetCurrentActualAgentState(Guid personId)
		{
			var agentState = queryActualAgentStates2(
				"SELECT * FROM Rta.ActualAgentState WHERE PersonId = @PersonId",
				new[]
				{
					new parameters
					{
						Name = "@PersonId",
						Value = personId
					}
				}).FirstOrDefault();
			return agentState;
		}

		[InfoLog]
		public virtual IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			return queryActualAgentStates2(@"
											SELECT * FROM RTA.ActualAgentState 
											WHERE OriginalDataSourceId = @datasource_id
											AND (
												BatchId < @batch_id
												OR 
												BatchId IS NULL
												)",
				new[]
				{
					new parameters
					{
						Name = "@datasource_id",
						Value = dataSourceId
					},
					new parameters
					{
						Name = "@batch_id",
						Value = batchId

					},
				});

		}

		private IEnumerable<AgentStateReadModel> queryActualAgentStates2(string sql, IEnumerable<parameters> parameters)
		{
			using (
				var connection = new SqlConnection(_connectionStrings.Analytics()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				
				command.CommandText = sql;
				parameters.ForEach(x => command.Parameters.AddWithValue(x.Name, x.Value));
				
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
							StateStartTime = reader.NullableDateTime("StateStartTime"),
							NextStart = reader.NullableDateTime("NextStart"),
							BatchId = reader.NullableDateTime("BatchId"),
							OriginalDataSourceId = reader.String("OriginalDataSourceId"),
							AdherenceStartTime = reader.NullableDateTime("AdherenceStartTime"),
							AlarmStartTime = reader.NullableDateTime("AlarmStartTime"),
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

		private class parameters
		{
			public string Name { get; set; }
			public object Value { get; set; }
		}
	}
}
