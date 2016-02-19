using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly IConnectionStrings _connectionStrings;
		private readonly ICurrentAnalyticsUnitOfWorkFactory _unitOfWorkFactory;

		public AgentStateReadModelReader(
			IConnectionStrings connectionStrings,
			ICurrentAnalyticsUnitOfWorkFactory unitOfWorkFactory
			)
		{
			_connectionStrings = connectionStrings;
			_unitOfWorkFactory = unitOfWorkFactory;
		}
		
		public IList<AgentStateReadModel> Load(IEnumerable<IPerson> persons)
        {
            var guids = persons.Select(person => person.Id.GetValueOrDefault()).ToList();
	        return Load(guids);
        }
		
		public IList<AgentStateReadModel> Load(IEnumerable<Guid> personGuids)
		{
            using (var uow = _unitOfWorkFactory.Current().CreateAndOpenStatelessUnitOfWork())
			{
				var ret = new List<AgentStateReadModel>();
				foreach (var personList in personGuids.Batch(400))
				{
					ret.AddRange(((NHibernateStatelessUnitOfWork)uow).Session
						.CreateSQLQuery(selectActualAgentState() + "WITH (NOLOCK) WHERE PersonId IN(:persons)")
						.SetParameterList("persons", personList)
						.SetResultTransformer(Transformers.AliasToBean(typeof(AgentStateReadModel)))
						.SetReadOnly(true)
						.List<AgentStateReadModel>());
				}
				return ret;
			}
		}
		
		public IList<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(selectActualAgentState() + "WITH (NOLOCK) WHERE TeamId = :teamId")
					.SetParameter("teamId", teamId)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
		}
		
		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			var query = selectActualAgentState() + @"WITH (NOLOCK) WHERE SiteId IN (:siteIds)";
			if (inAlarmOnly.HasValue)
				query += " AND IsRuleAlarm = " + Convert.ToInt32(inAlarmOnly.Value);
			if(alarmTimeDesc.HasValue)
				if (alarmTimeDesc.Value)
					query += " ORDER BY AlarmStartTime";
				else
					query += " ORDER BY AlarmStartTime DESC";
				
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(query)
					.SetParameterList("siteIds", siteIds)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			var query = selectActualAgentState() + @"WITH (NOLOCK) WHERE TeamId IN (:teamIds)";
			if (inAlarmOnly.HasValue)
				query += " AND IsRuleAlarm = " + Convert.ToInt32(inAlarmOnly.Value);
			if (alarmTimeDesc.HasValue)
				if (alarmTimeDesc.Value)
					query += " ORDER BY AlarmStartTime";
				else
					query += " ORDER BY AlarmStartTime DESC";

			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(query)
					.SetParameterList("teamIds", teamIds)
					.SetResultTransformer(Transformers.AliasToBean(typeof(AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
		}

		public IEnumerable<AgentStateReadModel> GetActualAgentStates()
		{
			return queryActualAgentStates(null, null);
		}

		[InfoLog]
		public virtual AgentStateReadModel GetCurrentActualAgentState(Guid personId)
		{
			var agentState = queryActualAgentStates("WHERE PersonId = @PersonId",
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
			return queryActualAgentStates(
				@"WHERE OriginalDataSourceId = @datasource_id
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
				})
				.ToArray();
		}

		private IEnumerable<AgentStateReadModel> queryActualAgentStates(string @where, IEnumerable<parameters> parameters)
		{
			var sql = selectActualAgentState() + (@where ?? "");
			using (var connection = new SqlConnection(_connectionStrings.Analytics()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				parameters
					.EmptyIfNull()
					.ForEach(x => command.Parameters.AddWithValue(x.Name, x.Value));
				
				connection.Open();
				using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (reader.Read())
					{
						yield return new AgentStateReadModel
						{
							PersonId = reader.Guid("PersonId"),
							BatchId = reader.NullableDateTime("BatchId"),
							OriginalDataSourceId = reader.String("OriginalDataSourceId"),
							PlatformTypeId = reader.Guid("PlatformTypeId"),
							BusinessUnitId = reader.NullableGuid("BusinessUnitId") ?? Guid.Empty,
							TeamId = reader.NullableGuid("TeamId"),
							SiteId = reader.NullableGuid("SiteId"),
							ReceivedTime = reader.DateTime("ReceivedTime"),

							ScheduledId = reader.NullableGuid("ScheduledId"),
							Scheduled = reader.String("Scheduled"),
							ScheduledNextId = reader.NullableGuid("ScheduledNextId"),
							ScheduledNext = reader.String("ScheduledNext"),
							NextStart = reader.NullableDateTime("NextStart"),

							StateCode = reader.String("StateCode"),
							StateId = reader.NullableGuid("StateId"),
							StateName = reader.String("StateName"),
							StateStartTime = reader.NullableDateTime("StateStartTime"),

							RuleId = reader.NullableGuid("RuleId"),
							RuleName = reader.String("RuleName"),
							RuleColor = reader.NullableInt("RuleColor"),
							RuleStartTime = reader.NullableDateTime("RuleStartTime"),
							StaffingEffect = reader.NullableDouble("StaffingEffect"),
							Adherence = reader.NullableInt("Adherence"),

							IsAlarm = reader.Boolean("IsAlarm"),
							AlarmStartTime = reader.NullableDateTime("AlarmStartTime"),
							AlarmColor = reader.NullableInt("AlarmColor"),
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

		private static string selectActualAgentState()
		{
			return @"SELECT 
						PersonId,
						BatchId,
						BusinessUnitId,
						SiteId,
						TeamId,
						OriginalDataSourceId,
						PlatformTypeId,
						ReceivedTime,

						Scheduled,
						ScheduledId,
						ScheduledNext,
						ScheduledNextId,
						NextStart,

						StateCode,
						State AS StateName,
						StateId,
						StateStartTime,

						AlarmId AS RuleId,
						AlarmName AS RuleName,
						Color AS RuleColor,
						RuleStartTime,
						StaffingEffect,
						Adherence,

						IsRuleAlarm AS IsAlarm,
						AlarmStartTime,
						AlarmColor

					FROM RTA.ActualAgentState ";
		}

	}
}
