using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
						.CreateSQLQuery(selectAgentState + "WITH (NOLOCK) WHERE PersonId IN(:persons)")
						.SetParameterList("persons", personList)
						)
					);
			}
			return ret;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(selectAgentState + "WITH (NOLOCK) WHERE TeamId = :teamId")
				.SetParameter("teamId", teamId)
				);
		}

		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds)
		{
			var query = selectAgentState + @"WITH (NOLOCK) WHERE SiteId IN (:siteIds)";
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("siteIds", siteIds)
				);
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds)
		{
			var query = selectAgentState + @"WITH (NOLOCK) WHERE TeamId IN (:teamIds)";
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("teamIds", teamIds)
				);
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds)
		{
			var query = selectAgentState + @"WITH (NOLOCK) WHERE SiteId IN (:siteIds) AND AlarmStarttime <= :now ORDER BY AlarmStartTime ASC";
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("siteIds", siteIds)
				.SetParameter("now", _now.UtcDateTime())
				);
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds)
		{
			var query = selectAgentState + @"WITH (NOLOCK) WHERE TeamId IN (:teamIds) AND AlarmStartTime <= :now ORDER BY AlarmStartTime ASC";
			return transform(_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameterList("teamIds", teamIds)
				.SetParameter("now", _now.UtcDateTime())
				);
		}

		private const string agentsForSkillQuery = @"
SELECT
	a.[PersonId],
	a.[BusinessUnitId],
	a.[SiteId],
	a.[TeamId],
	a.[ReceivedTime],
	a.[Activity],
	a.[NextActivity],
	a.[NextActivityStartTime],
	a.[StateCode],
	a.[StateName],
	a.[StateStartTime],
	a.[RuleName],
	a.[RuleStartTime],
	a.[RuleColor],
	a.[StaffingEffect],
	a.[IsRuleAlarm],
	a.[AlarmStartTime],
	a.[AlarmColor],
	a.[Shift]
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
	ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId = :skillId
AND :today BETWEEN g.StartDate and g.EndDate ";
		private const string hardcodedSkillGroupingPageId = "4CE00B41-0722-4B36-91DD-0A3B63C545CF";
		public IEnumerable<AgentStateReadModel> LoadForSkill(Guid skillId)
		{
			return transform(
				_unitOfWork.Current().Session()
				.CreateSQLQuery(agentsForSkillQuery)
				.SetParameter("skillId", skillId)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("skillGroupingPageId", hardcodedSkillGroupingPageId)
				);
		}
		public IEnumerable<AgentStateReadModel> LoadAlarmsForSkill(Guid skillId)
		{
			var query = agentsForSkillQuery + " AND AlarmStartTime <= :now ORDER BY AlarmStartTime ASC ";
			return transform(
				_unitOfWork.Current().Session()
				.CreateSQLQuery(query)
				.SetParameter("skillId", skillId)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("skillGroupingPageId", hardcodedSkillGroupingPageId)
				);
		}

		private IEnumerable<AgentStateReadModel> transform(IQuery query)
		{
			return query
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.SetReadOnly(true)
				.List<internalModel>()
				.Select(x =>
				{
					(x as AgentStateReadModel).Shift = _deserializer.DeserializeObject<AgentStateActivityReadModel[]>(x.Shift);
					x.Shift = null;
					(x as AgentStateReadModel).OutOfAdherences = _deserializer.DeserializeObject<AgentStateOutOfAdherenceReadModel[]>(x.OutOfAdherences);
					x.OutOfAdherences = null;
					return x;
				})
				.ToArray();
		}

		private static readonly string selectAgentState = @"SELECT * FROM [ReadModel].AgentState ";

		private class internalModel : AgentStateReadModel
		{
			public new string Shift { get; set; }
			public new string OutOfAdherences { get; set; }
		}
	}
}
