using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;

		public AgentStateReadModelReader(ICurrentAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IList<AgentStateReadModel> Load(IEnumerable<IPerson> persons)
        {
			return Load(
				persons
					.Select(person => person.Id.GetValueOrDefault())
					.ToList()
				);
        }

		[AnalyticsUnitOfWork]
		public virtual IList<AgentStateReadModel> Load(IEnumerable<Guid> personIds)
		{
			var ret = new List<AgentStateReadModel>();
			foreach (var personList in personIds.Batch(400))
			{
				ret.AddRange(_unitOfWork.Current().Session()
					.CreateSQLQuery(selectActualAgentState() + "WITH (NOLOCK) WHERE PersonId IN(:persons)")
					.SetParameterList("persons", personList)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>());
			}
			return ret;
		}

		[AnalyticsUnitOfWork]
		public virtual IList<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(selectActualAgentState() + "WITH (NOLOCK) WHERE TeamId = :teamId")
				.SetParameter("teamId", teamId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		[AnalyticsUnitOfWork]
		public virtual IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			var query = selectActualAgentState() + @"WITH (NOLOCK) WHERE SiteId IN (:siteIds)";
			if (inAlarmOnly.HasValue)
				query += " AND IsRuleAlarm = " + Convert.ToInt32(inAlarmOnly.Value);
			if(alarmTimeDesc.HasValue)
				if (alarmTimeDesc.Value)
					query += " ORDER BY AlarmStartTime";
				else
					query += " ORDER BY AlarmStartTime DESC";

			return _unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("siteIds", siteIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		[AnalyticsUnitOfWork]
		public virtual IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds, bool? inAlarmOnly, bool? alarmTimeDesc)
		{
			var query = selectActualAgentState() + @"WITH (NOLOCK) WHERE TeamId IN (:teamIds)";
			if (inAlarmOnly.HasValue)
				query += " AND IsRuleAlarm = " + Convert.ToInt32(inAlarmOnly.Value);
			if (alarmTimeDesc.HasValue)
				if (alarmTimeDesc.Value)
					query += " ORDER BY AlarmStartTime";
				else
					query += " ORDER BY AlarmStartTime DESC";

			return _unitOfWork.Current().Session().CreateSQLQuery(query)
				.SetParameterList("teamIds", teamIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		[AnalyticsUnitOfWork]
		public virtual IEnumerable<AgentStateReadModel> GetActualAgentStates()
		{
			var sql = selectActualAgentState();
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		[InfoLog]
		[AnalyticsUnitOfWork]
		public virtual AgentStateReadModel GetCurrentActualAgentState(Guid personId)
		{
			var sql = selectActualAgentState() + "WHERE PersonId = :PersonId";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>()
				.FirstOrDefault();
		}

		[InfoLog]
		[AnalyticsUnitOfWork]
		public virtual IEnumerable<AgentStateReadModel> GetAgentsNotInSnapshot(DateTime batchId, string dataSourceId)
		{
			var sql = selectActualAgentState() +
					  @"WHERE 
						OriginalDataSourceId = :OriginalDataSourceId
						AND (
							BatchId < :BatchId
							OR 
							BatchId IS NULL
							)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("BatchId", batchId)
				.SetParameter("OriginalDataSourceId", dataSourceId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
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
