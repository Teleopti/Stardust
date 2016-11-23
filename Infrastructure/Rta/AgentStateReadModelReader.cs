using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelReader : IAgentStateReadModelReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public AgentStateReadModelReader(ICurrentUnitOfWork unitOfWork, INow now, HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId)
		{
			_unitOfWork = unitOfWork;
			_now = now;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
		}

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons)
		{
			return Read(
				persons
					.Select(person => person.Id.GetValueOrDefault())
					.ToList()
				);
		}

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds)
		{
			return personIds.Batch(400).SelectMany(personIdBatch => _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE PersonId IN(:persons)")
				.SetParameterList("persons", personIdBatch)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>())
				.ToArray();
		}

		public IEnumerable<AgentStateReadModel> ReadForTeam(Guid teamId)
		{
			return ReadForTeams(new[] { teamId });
		}
		
		public IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			var queryBuilder = new AgentStateReadModelQueryBuilder(_now);
			if (siteIds != null)
				queryBuilder.InSites(siteIds);
			if (teamIds != null)
				queryBuilder.InTeams(teamIds);
			if (skillIds != null)
				queryBuilder.WithSkills(skillIds);

			var builder = queryBuilder.Build();
			var sqlQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery(builder.Query);
			builder.ParameterFuncs.ForEach(f => f(sqlQuery));
			return sqlQuery
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadForSites(IEnumerable<Guid> siteIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE SiteId IN (:siteIds)")
				.SetParameterList("siteIds", siteIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE TeamId IN (:teamIds)")
				.SetParameterList("teamIds", teamIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadForSkills(IEnumerable<Guid> skillIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate")
				.SetParameterList("skillIds", skillIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT a.*
FROM ReadModel.AgentState AS a
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE a.SiteId IN (:SiteIds)
AND g.PageId = :skillGroupingPageId
AND g.GroupId IN (:SkillIds)
AND :today BETWEEN g.StartDate and g.EndDate")
				.SetParameterList("SiteIds", siteIds)
				.SetParameterList("SkillIds", skillIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT a.*
FROM ReadModel.AgentState AS a
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE a.TeamId IN (:TeamIds)
AND g.PageId = :skillGroupingPageId
AND g.GroupId IN (:SkillIds)
AND :today BETWEEN g.StartDate and g.EndDate")
				.SetParameterList("TeamIds", teamIds)
				.SetParameterList("SkillIds", skillIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}


		public IEnumerable<AgentStateReadModel> ReadInAlarmsForSites(IEnumerable<Guid> siteIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE SiteId IN (:siteIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
				.SetParameterList("siteIds", siteIds)
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmsForTeams(IEnumerable<Guid> teamIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE TeamId IN (:teamIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
				.SetParameterList("teamIds", teamIds)
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmsForSkills(IEnumerable<Guid> skillIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
ORDER BY AlarmStartTime ASC")
				.SetParameterList("skillIds", skillIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmsForSitesAndSkills(IEnumerable<Guid> siteIds,
			IEnumerable<Guid> skillIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND a.SiteId IN (:siteIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
ORDER BY AlarmStartTime ASC")
				.SetParameterList("skillIds", skillIds)
				.SetParameterList("siteIds", siteIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmsForTeamsAndSkills(IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND a.TeamId IN (:teamIds)
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
ORDER BY AlarmStartTime ASC")
				.SetParameterList("teamIds", teamIds)
				.SetParameterList("skillIds", skillIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}


		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSites(IEnumerable<Guid> siteIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE StateGroupId IS NOT NULL
AND SiteId IN (:siteIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
					.SetParameterList("siteIds", siteIds)
					.SetParameter("now", _now.UtcDateTime())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE StateGroupId NOT IN (:excludedStateGroupIds)
AND StateGroupId IS NOT NULL
AND SiteId IN (:siteIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
					.SetParameterList("siteIds", siteIds)
					.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
					.SetParameter("now", _now.UtcDateTime())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}

			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE
(
	StateGroupId NOT IN (:excludedStateGroupIds)
	OR StateGroupId IS NULL
)
AND SiteId IN (:siteIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
				.SetParameterList("siteIds", siteIds)
				.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeams(IEnumerable<Guid> teamIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE StateGroupId IS NOT NULL
AND TeamId IN (:teamIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
					.SetParameterList("teamIds", teamIds)
					.SetParameter("now", _now.UtcDateTime())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE StateGroupId NOT IN (:excludedStateGroupIds)
AND StateGroupId IS NOT NULL
AND TeamId IN (:teamIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
					.SetParameterList("teamIds", teamIds)
					.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
					.SetParameter("now", _now.UtcDateTime())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}

			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT TOP 50 *
FROM [ReadModel].AgentState WITH (NOLOCK)
WHERE
(
	StateGroupId NOT IN (:excludedStateGroupIds)
	OR StateGroupId IS NULL
)
AND TeamId IN (:teamIds)
AND AlarmStarttime <= :now
ORDER BY AlarmStartTime ASC")
				.SetParameterList("teamIds", teamIds)
				.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSkills(IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND StateGroupId IS NOT NULL
ORDER BY AlarmStartTime ASC")
					.SetParameterList("skillIds", skillIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("now", _now.UtcDateTime())
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND StateGroupId NOT IN (:excludedStateGroupIds)
AND StateGroupId IS NOT NULL
ORDER BY AlarmStartTime ASC")
					.SetParameterList("skillIds", skillIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("now", _now.UtcDateTime())
					.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}

			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND
(
	StateGroupId NOT IN (:excludedStateGroupIds)
	OR StateGroupId IS NULL
)
ORDER BY AlarmStartTime ASC")
				.SetParameterList("skillIds", skillIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSitesAndSkill(IEnumerable<Guid> siteIds,
			IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND a.SiteId IN (:siteIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND StateGroupId IS NOT NULL
ORDER BY AlarmStartTime ASC")
					.SetParameterList("skillIds", skillIds)
					.SetParameterList("siteIds", siteIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("now", _now.UtcDateTime())
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND a.SiteId IN (:siteIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND StateGroupId NOT IN (:excludedStateGroupIds)
AND StateGroupId IS NOT NULL
ORDER BY AlarmStartTime ASC")
					.SetParameterList("skillIds", skillIds)
					.SetParameterList("siteIds", siteIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("now", _now.UtcDateTime())
					.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND a.SiteId IN (:siteIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND
(
	StateGroupId NOT IN (:excludedStateGroupIds)
	OR StateGroupId IS NULL
)
ORDER BY AlarmStartTime ASC
")
				.SetParameterList("skillIds", skillIds)
				.SetParameterList("siteIds", siteIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameterList("excludedStateGroupIds", excludedStateGroupIds)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeamsAndSkill(IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			var stateGroupIdsWithoutNull = excludedStateGroupIds.Where(x => x != null);
			if (excludedStateGroupIds.All(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND a.TeamId IN (:teamIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND StateGroupId IS NOT NULL
ORDER BY AlarmStartTime ASC")
					.SetParameterList("skillIds", skillIds)
					.SetParameterList("teamIds", teamIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("now", _now.UtcDateTime())
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
			if (excludedStateGroupIds.Any(x => x == null))
			{
				return _unitOfWork.Current().Session()
					.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND a.TeamId IN (:teamIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND StateGroupId NOT IN (:excludedStateGroupIds)
AND StateGroupId IS NOT NULL
ORDER BY AlarmStartTime ASC")
					.SetParameterList("skillIds", skillIds)
					.SetParameterList("teamIds", teamIds)
					.SetParameter("today", _now.UtcDateTime().Date)
					.SetParameter("now", _now.UtcDateTime())
					.SetParameterList("excludedStateGroupIds", stateGroupIdsWithoutNull)
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
					.SetReadOnly(true)
					.List<AgentStateReadModel>();
			}
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT DISTINCT TOP 50 a.*
FROM ReadModel.AgentState AS a WITH (NOLOCK)
INNER JOIN ReadModel.GroupingReadOnly AS g
ON a.PersonId = g.PersonId
WHERE PageId = :skillGroupingPageId
AND g.GroupId IN (:skillIds)
AND a.TeamId IN (:teamIds)
AND :today BETWEEN g.StartDate and g.EndDate
AND AlarmStartTime <= :now
AND
(
	StateGroupId NOT IN (:excludedStateGroupIds)
	OR StateGroupId IS NULL
)
ORDER BY AlarmStartTime ASC
")
				.SetParameterList("skillIds", skillIds)
				.SetParameterList("teamIds", teamIds)
				.SetParameter("today", _now.UtcDateTime().Date)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameterList("excludedStateGroupIds", excludedStateGroupIds)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}




		private class internalModel : AgentStateReadModel
		{
			public new string Shift
			{
				set { base.Shift = value != null ? JsonConvert.DeserializeObject<AgentStateActivityReadModel[]>(value) : null; }
			}

			public new string OutOfAdherences
			{
				set { base.OutOfAdherences = value != null ? JsonConvert.DeserializeObject<AgentStateOutOfAdherenceReadModel[]>(value) : null; }
			}
		}
	}

	public class HardcodedSkillGroupingPageId
	{
		private const string id = "4CE00B41-0722-4B36-91DD-0A3B63C545CF";

		public static string Id => id;
		
		public string Get()
		{
			return id;
		}
	}
}
