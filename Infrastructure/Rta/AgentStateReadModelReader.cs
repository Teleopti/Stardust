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

		public AgentStateReadModelReader(ICurrentUnitOfWork unitOfWork, INow now)
		{
			_unitOfWork = unitOfWork;
			_now = now;
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
			return personIds.Batch(400).SelectMany(personIdBatch => _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE PersonId IN(:persons)")
				.SetParameterList("persons", personIdBatch)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>())
				.ToArray();
		}

		public IEnumerable<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			return LoadForTeams(new[] {teamId});
		}





		
		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE SiteId IN (:siteIds)")
				.SetParameterList("siteIds", siteIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].AgentState WITH (NOLOCK) WHERE TeamId IN (:teamIds)")
				.SetParameterList("teamIds", teamIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> LoadForSkills(IEnumerable<Guid> skillIds)
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
				.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> LoadForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AgentStateReadModel> LoadForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			throw new NotImplementedException();
		}


		public IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds)
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

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds)
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

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSkills(IEnumerable<Guid> skillIds)
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
				.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeamsAndSkills(IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSitesAndSkills(IEnumerable<Guid> siteIds,
			IEnumerable<Guid> skillIds)
		{
			throw new NotImplementedException();
		}


		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSites(IEnumerable<Guid> siteIds,
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

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForTeams(IEnumerable<Guid> teamIds,
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

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSkills(IEnumerable<Guid> skillIds,
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
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
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
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
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
				.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>();
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSitesAndSkill(IEnumerable<Guid> siteIds,
			IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			throw new NotImplementedException();
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

	public static class HardcodedSkillGroupingPageId
	{
		public const string Get = "4CE00B41-0722-4B36-91DD-0A3B63C545CF";
	}
}
