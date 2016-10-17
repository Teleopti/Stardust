﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		private readonly IJsonDeserializer _deserializer;

		public AgentStateReadModelReader(ICurrentUnitOfWork unitOfWork, INow now, IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_now = now;
			_deserializer = deserializer;
		}

		public IEnumerable<AgentStateReadModel> Load(IEnumerable<IPerson> persons)
		{
			return Load(
				persons
					.Select(person => person.Id.GetValueOrDefault())
					.ToList()
				);
		}

		public IEnumerable<AgentStateReadModel> Load(IEnumerable<Guid> personIds)
		{
			var ret = new List<AgentStateReadModel>();
			foreach (var personList in personIds.Batch(400))
			{
				ret.AddRange(
					transform(_unitOfWork.Current().Session()
						.CreateSQLQuery(string.Format(selectAgentState, "", "WITH (NOLOCK) WHERE PersonId IN(:persons)"))
						.SetParameterList("persons", personList)
						)
					);
			}
			return ret;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(string.Format(selectAgentState, "", "WITH (NOLOCK) WHERE TeamId = :teamId"))
				.SetParameter("teamId", teamId)
				);
		}








		private const string selectAgentState = @"SELECT {0} * FROM [ReadModel].AgentState {1}";

		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds)
		{
			var query = string.Format(selectAgentState, "", @"WITH (NOLOCK) WHERE SiteId IN (:siteIds)");
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("siteIds", siteIds)
				);
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds)
		{
			var query = string.Format(selectAgentState, "", @"WITH (NOLOCK) WHERE TeamId IN (:teamIds)");
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("teamIds", teamIds)
				);
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds)
		{
			var query = string.Format(selectAgentState, " TOP 50 ",
				@"WITH (NOLOCK) WHERE SiteId IN (:siteIds) AND AlarmStarttime <= :now ORDER BY AlarmStartTime ASC");
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("siteIds", siteIds)
				.SetParameter("now", _now.UtcDateTime())
				);
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds)
		{
			var query = string.Format(selectAgentState, " TOP 50 ",
				@"WITH (NOLOCK) WHERE TeamId IN (:teamIds) AND AlarmStartTime <= :now ORDER BY AlarmStartTime ASC");
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("teamIds", teamIds)
				.SetParameter("now", _now.UtcDateTime())
				);
		}

		private const string selectAgentStateInAlarm =
			@"SELECT TOP 50 * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE {0} AND AlarmStartTime <= :now ORDER BY AlarmStartTime ASC";

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds, IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return transform(
					createSQLQuery(string.Format(selectAgentStateInAlarm,
						"StateGroupId IS NOT NULL AND SiteId IN (:siteIds)"),
						q => q.SetParameterList("siteIds", siteIds)));
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return transform(
					createSQLQuery(string.Format(selectAgentStateInAlarm,
						"StateGroupId NOT IN (:excludedStateGroupIds) AND StateGroupId IS NOT NULL AND SiteId IN (:siteIds)"),
						q => q.SetParameterList("siteIds", siteIds)
							.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)));
			}
			return transform(
				createSQLQuery(string.Format(selectAgentStateInAlarm,
					"(StateGroupId NOT IN (:excludedStateGroupIds) OR StateGroupId IS NULL) AND SiteId IN (:siteIds)"),
					q => q.SetParameterList("siteIds", siteIds)
						.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)));
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds, IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return transform(
					createSQLQuery(string.Format(selectAgentStateInAlarm,
						"StateGroupId IS NOT NULL AND TeamId IN (:teamIds)"),
						q => q.SetParameterList("teamIds", teamIds)));
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return transform(
					createSQLQuery(string.Format(selectAgentStateInAlarm,
						"StateGroupId NOT IN (:excludedStateGroupIds) AND StateGroupId IS NOT NULL AND TeamId IN (:teamIds)"),
						q => q.SetParameterList("teamIds", teamIds)
							.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)));
			}
			return transform(
				createSQLQuery(string.Format(selectAgentStateInAlarm,
					"(StateGroupId NOT IN (:excludedStateGroupIds) OR StateGroupId IS NULL) AND TeamId IN (:teamIds)"),
					q => q.SetParameterList("teamIds", teamIds)
						.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)));
		}

		private IQuery createSQLQuery(string query, Func<IQuery, IQuery> func)
		{
			return func(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameter("now", _now.UtcDateTime()));
		}

		private const string agentsForSkillQuery = @"
SELECT DISTINCT {0}
	a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
	ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate 
{1}";
		public const string HardcodedSkillGroupingPageId = "4CE00B41-0722-4B36-91DD-0A3B63C545CF";

		public IEnumerable<AgentStateReadModel> LoadForSkills(IEnumerable<Guid> skillIds)
		{
			return transform(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(string.Format(agentsForSkillQuery, "", ""))
					.SetParameterList("skillIds", skillIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId)
				);
		}

		private const string agentsInAlarmForSkillQuery = @"
SELECT DISTINCT TOP 50
	a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
	ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate 
{0}
AND AlarmStartTime <= :now ORDER BY AlarmStartTime ASC ";
		public IEnumerable<AgentStateReadModel> LoadAlarmsForSkills(IEnumerable<Guid> skillIds)
		{
			var query = string.Format(agentsInAlarmForSkillQuery, "");
			return transform(
				_unitOfWork.Current().Session()
					.CreateSQLQuery(query)
					.SetParameterList("skillIds", skillIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("now", _now.UtcDateTime())
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId)
				);
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSkills(IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return transform(
					createSQLQueryForSkill(string.Format(agentsInAlarmForSkillQuery,
						" AND StateGroupId IS NOT NULL "),
						skillIds, 
						q => q));
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return transform(
					createSQLQueryForSkill(string.Format(agentsInAlarmForSkillQuery,
						" AND StateGroupId NOT IN (:excludedStateGroupIds) AND StateGroupId IS NOT NULL "),
						skillIds, 
						q => q.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)));
			}
			return transform(
				createSQLQueryForSkill(string.Format(agentsInAlarmForSkillQuery, 
				" AND (StateGroupId NOT IN (:excludedStateGroupIds) OR StateGroupId IS NULL) "),
				skillIds, 
				q => q.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)));
		}

		private IQuery createSQLQueryForSkill(string query, IEnumerable<Guid> skillIds, Func<IQuery, IQuery> func)
		{
			return func(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameterList("skillIds", skillIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId));
		}

		private IEnumerable<AgentStateReadModel> transform(IQuery query)
		{
			return query
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<internalModel>()
				.Select(x =>
				{
					(x as AgentStateReadModel).Shift = _deserializer.DeserializeObject<AgentStateActivityReadModel[]>(x.Shift);
					x.Shift = null;
					(x as AgentStateReadModel).OutOfAdherences =
						_deserializer.DeserializeObject<AgentStateOutOfAdherenceReadModel[]>(x.OutOfAdherences);
					x.OutOfAdherences = null;
					return x;
				})
				.ToArray();
		}

		private class internalModel : AgentStateReadModel
		{
			public new string Shift { get; set; }
			public new string OutOfAdherences { get; set; }
		}
	}
}
